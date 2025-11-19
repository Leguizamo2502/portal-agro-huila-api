using Business.Interfaces.Implements.Orders.OrderChat;
using Data.Interfaces.Implements.Orders;
using Data.Interfaces.Implements.Orders.OrderChat;
using Data.Interfaces.Implements.Producers;
using Entity.Domain.Models.Implements.Orders;
using Entity.Domain.Models.Implements.Orders.ChatOrder;
using Entity.DTOs.Orders.OrderChat;
using Utilities.Exceptions;

namespace Business.Services.Orders.OrderChat
{
    public class OrderChatService : IOrderChatService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderChatConversationRepository _conversationRepository;
        private readonly IOrderChatMessageRepository _messageRepository;
        private readonly IProducerRepository _producerRepository;
        private readonly IOrderChatMessagePusher _messagePusher;

        private const int MaxPageSize = 100;

        public OrderChatService(
            IOrderRepository orderRepository,
            IOrderChatConversationRepository conversationRepository,
            IOrderChatMessageRepository messageRepository,
            IProducerRepository producerRepository,
            IOrderChatMessagePusher messagePusher)
        {
            _orderRepository = orderRepository;
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _producerRepository = producerRepository;
            _messagePusher = messagePusher;
        }

        public async Task EnsureConversationForOrderAsync(int orderId)
        {
            var existing = await _conversationRepository.GetByOrderIdAsync(orderId);
            if (existing != null)
            {
                return;
            }

            var conversation = new OrderChatConversation
            {
                OrderId = orderId
            };

            await _conversationRepository.AddAsync(conversation);
        }

        public async Task AddSystemMessageAsync(int orderId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var order = await _orderRepository.GetByIdAsync(orderId)
                        ?? throw new BusinessException("Orden no encontrada para el chat.");

            await EnsureConversationForOrderAsync(order.Id);
            var conversation = await _conversationRepository.GetByOrderIdAsync(order.Id)
                               ?? throw new BusinessException("La conversación del pedido no existe.");

            var participants = await GetParticipantsAsync(order);

            var entry = new OrderChatMessage
            {
                ConversationId = conversation.Id,
                IsSystem = true,
                Message = message.Trim(),
                SenderUserId = 0
            };

            var saved = await _messageRepository.AddAsync(entry);
            await BroadcastMessageAsync(order.Code, participants, saved);
        }

        public async Task EnsureParticipantAsync(int userId, string orderCode)
        {
            var order = await _orderRepository.GetByCode(orderCode)
                        ?? throw new BusinessException("Orden no encontrada.");

            var participants = await GetParticipantsAsync(order);
            if (!participants.IsParticipant(userId))
            {
                throw new BusinessException("No estás autorizado para este chat.");
            }

            await EnsureConversationForOrderAsync(order.Id);
        }

        public async Task<OrderChatMessageDto> SendMessageAsync(int userId, string orderCode, OrderChatMessageCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var message = dto.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new BusinessException("El mensaje no puede estar vacío.");
            }

            var order = await _orderRepository.GetByCode(orderCode)
                        ?? throw new BusinessException("Orden no encontrada.");

            var participants = await GetParticipantsAsync(order);
            if (!participants.IsParticipant(userId))
            {
                throw new BusinessException("No estás autorizado para enviar mensajes en este chat.");
            }

            await EnsureConversationForOrderAsync(order.Id);
            var conversation = await _conversationRepository.GetByOrderIdAsync(order.Id)
                               ?? throw new BusinessException("No se pudo acceder al chat del pedido.");

            var entry = new OrderChatMessage
            {
                ConversationId = conversation.Id,
                Message = message,
                SenderUserId = userId,
                IsSystem = false
            };

            var saved = await _messageRepository.AddAsync(entry);

            await BroadcastMessageAsync(order.Code, participants, saved);

            return MapToDto(saved, participants, userId);
        }

        public async Task<OrderChatMessagesPageDto> GetMessagesAsync(int userId, string orderCode, int skip, int take)
        {
            var order = await _orderRepository.GetByCode(orderCode)
                        ?? throw new BusinessException("Orden no encontrada.");

            var participants = await GetParticipantsAsync(order);
            if (!participants.IsParticipant(userId))
            {
                throw new BusinessException("No estás autorizado para ver este chat.");
            }

            var sanitizedSkip = Math.Max(0, skip);
            var sanitizedTake = Math.Clamp(take <= 0 ? 50 : take, 1, MaxPageSize);

            await EnsureConversationForOrderAsync(order.Id);
            var conversation = await _conversationRepository.GetByOrderIdAsync(order.Id)
                               ?? throw new BusinessException("No se pudo acceder al chat del pedido.");

            var total = await _messageRepository.CountAsync(conversation.Id);
            var messages = await _messageRepository.GetMessagesAsync(conversation.Id, sanitizedSkip, sanitizedTake);
            var dtos = messages.Select(m => MapToDto(m, participants, userId)).ToList();

            return new OrderChatMessagesPageDto
            {
                OrderId = order.Id,
                OrderCode = order.Code,
                ConversationId = conversation.Id,
                Total = total,
                HasMore = sanitizedSkip + dtos.Count < total,
                Messages = dtos
            };
        }

        private async Task<OrderParticipants> GetParticipantsAsync(Order order)
        {
            var contact = await _producerRepository.GetContactProducer(order.ProducerIdSnapshot)
                          ?? throw new BusinessException("No se encontró el productor asociado al pedido.");

            if (contact.UserId == null)
            {
                throw new BusinessException("El productor no tiene un usuario asociado.");
            }

            return new OrderParticipants(order.UserId, contact.UserId.Value);
        }

        private async Task BroadcastMessageAsync(string orderCode, OrderParticipants participants, OrderChatMessage message)
        {
            if (_messagePusher == null)
            {
                return;
            }

            var customerDto = MapToDto(message, participants, participants.CustomerUserId);
            var producerDto = participants.CustomerUserId == participants.ProducerUserId
                ? customerDto
                : MapToDto(message, participants, participants.ProducerUserId);

            await _messagePusher.BroadcastMessageAsync(orderCode, customerDto, producerDto, participants.CustomerUserId, participants.ProducerUserId);
        }

        private static OrderChatMessageDto MapToDto(OrderChatMessage message, OrderParticipants participants, int requesterUserId)
        {
            var senderType = participants.ResolveRole(message.SenderUserId);
            var isMine = message.SenderUserId == requesterUserId;

            return new OrderChatMessageDto
            {
                Id = message.Id,
                Message = message.Message,
                SentAtUtc = message.CreateAt,
                SenderUserId = message.SenderUserId,
                SenderType = senderType,
                IsSystem = message.IsSystem,
                IsMine = isMine
            };
        }

        private sealed record OrderParticipants(int CustomerUserId, int ProducerUserId)
        {
            public bool IsParticipant(int userId) => userId == CustomerUserId || userId == ProducerUserId;

            public string ResolveRole(int userId)
            {
                if (userId == 0)
                {
                    return "System";
                }

                if (userId == CustomerUserId)
                {
                    return "Customer";
                }

                if (userId == ProducerUserId)
                {
                    return "Producer";
                }

                return "Unknown";
            }
        }
    }
}
