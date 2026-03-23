using Microsoft.AspNetCore.SignalR;
using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagement.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Khi một người mở Web lên
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // Logic: Cập nhật IsOnline = true vào Database nếu cần
                // Hoặc đơn giản là thông báo cho mọi người biết mình vừa online
                await Clients.Others.SendAsync("UserOnline", userId);

                if (Context.User.IsInRole("Admin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
            }
            await base.OnConnectedAsync();
        }

        // Khi một người tắt Web/Đăng xuất
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // Logic: Cập nhật IsOnline = false vào Database
                await Clients.Others.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Gửi tin nhắn và lưu vào Database
        public async Task SendMessage(string senderName, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            senderName = string.IsNullOrWhiteSpace(senderName) ? "Admin" : senderName.Trim();
            var content = message.Trim();
            var sentAt = DateTime.Now;
            var time = sentAt.ToString("HH:mm");

            try
            {
                await EnsureMessagesTableAsync();

                var row = new Message
                {
                    SenderName = senderName,
                    Content = content,
                    SentAt = sentAt
                };
                _context.Messages.Add(row);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist chat message to Messages table.");
            }

            await Clients.All.SendAsync("ReceiveMessage", senderName, content, time);

            await Clients.Others.SendAsync("ReceiveNotification");
        }

        public async Task<object[]> GetRecentMessages(int take = 100)
        {
            if (take <= 0) take = 50;
            if (take > 300) take = 300;

            try
            {
                await EnsureMessagesTableAsync();

                var rows = await _context.Messages
                    .AsNoTracking()
                    .OrderByDescending(x => x.MessageId)
                    .Take(take)
                    .OrderBy(x => x.MessageId)
                    .Select(x => new
                    {
                        x.SenderName,
                        x.Content,
                        x.SentAt
                    })
                    .ToListAsync();

                return rows
                    .Select(x => (object)new
                    {
                        user = x.SenderName,
                        message = x.Content,
                        time = x.SentAt.ToString("HH:mm")
                    })
                    .ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read chat history from Messages table.");
                return Array.Empty<object>();
            }
        }

        private async Task EnsureMessagesTableAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'dbo.Messages', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Messages](
        [MessageId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SenderName] [nvarchar](100) NOT NULL,
        [Content] [nvarchar](1000) NOT NULL,
        [SentAt] [datetime] NOT NULL CONSTRAINT [DF_Messages_SentAt] DEFAULT (GETDATE())
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Messages', 'SenderName') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Messages] ADD [SenderName] [nvarchar](100) NULL;
        UPDATE [dbo].[Messages] SET [SenderName] = N'Unknown' WHERE [SenderName] IS NULL;
        ALTER TABLE [dbo].[Messages] ALTER COLUMN [SenderName] [nvarchar](100) NOT NULL;
    END

    IF COL_LENGTH('dbo.Messages', 'SentAt') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Messages] ADD [SentAt] [datetime] NULL;
        UPDATE [dbo].[Messages] SET [SentAt] = GETDATE() WHERE [SentAt] IS NULL;
        ALTER TABLE [dbo].[Messages] ALTER COLUMN [SentAt] [datetime] NOT NULL;
    END

    IF OBJECT_ID(N'DF_Messages_SentAt', N'D') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Messages] ADD CONSTRAINT [DF_Messages_SentAt] DEFAULT (GETDATE()) FOR [SentAt];
    END
END
");
        }
    }
}