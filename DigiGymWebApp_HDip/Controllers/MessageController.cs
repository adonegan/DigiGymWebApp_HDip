using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "ClientOrTrainer")]
    public class MessageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MessageController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
            _context.Database.EnsureCreated();
        }

        public async Task<IActionResult> Messages()
        {
            // collect user's info
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            // check if current logged-in user is a trainer
            if (User.IsInRole("Trainer"))
            {
                // Trainers see all messages related to their conversations
                var conversations = await _context.Messages
                    .Where(m => m.SenderID == userId || m.ReceiverID == userId)
                    .Include(m => m.Conversation)
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .ToListAsync();

                var trainerConversations = conversations
                    .GroupBy(m => m.ConversationID)
                    .Select(g => g.First())
                    .ToList();

                // Trainer-specific view
                return View("TrainerMessages", trainerConversations); 
            }
            else
            {
                // if current logged-in user is a client
                // they see only messages related to them
                var conversations = await _context.Messages
                    .Where(m => m.ReceiverID == userId || m.SenderID == userId)
                    .Include(m => m.Conversation)
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .ToListAsync();

                var clientConversations = conversations
                    .Where(m => m.SenderID == userId || m.ReceiverID != userId)
                    .GroupBy(m => m.ConversationID)
                    .Select(g => g.First())
                    .ToList();

                // show unread messages to the client
                var unreadMessages = conversations.Where(m => m.ReceiverID == userId && !m.IsRead).ToList();
                ViewBag.UnreadMessages = unreadMessages;

                // Client-specific view
                return View("ClientMessages", clientConversations); 
            }
        }

        public async Task<IActionResult> Create(int? convoID)
        {
            // if convoID - set in <input> field as ConversationID in form - has a value
            if (convoID.HasValue)
            {
                // query db to find a conversationID that matches the convoID param / arg
                var conversation = _context.Conversations
                    .FirstOrDefault(c => c.ConversationID == convoID.Value);

                // pass conversation info to view
                if (conversation != null)
                {
                    ViewBag.ConversationInfo = conversation; 
                }
            }
            return View("Create");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,ConversationID")] Message message)
        {
            if (ModelState.IsValid)
            {
                // get currently logged-in user details
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);

                // check if currently logged-in user is a client usertype
                var isClient = await _userManager.IsInRoleAsync(user, "Client");

                // new conversation
                if (message.ConversationID == 0) 
                {
                    // only clients can start new conversations
                    // trainers are null because they haven't replied yet
                    var conversation = new Conversation
                    {
                        // if currently logged-in user a client, set ClientID to their userId, otherwise set to null
                        ClientID = isClient ? userId : null,
                        TrainerID = null
                    };

                    // add conversation and save to db
                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();

                    // set conversationID in message table to conversationID from conversation
                    message.ConversationID = conversation.ConversationID;
                }

                // set additional fields for message
                message.SenderID = userId;
                message.Timestamp = DateTime.Now;

                // add and save to db
                _context.Add(message);
                await _context.SaveChangesAsync();

                // redirect to Messages action - ClientMessages.cshtml if client, TrainerMessages.cshtml if trainer
                return RedirectToAction(nameof(Messages));
            }

            // in case of model validation errors, repopulate conversation info
            if (message.ConversationID != 0)
            {
                var conversation = _context.Conversations
                    .FirstOrDefault(c => c.ConversationID == message.ConversationID);

                if (conversation != null)
                {
                    ViewBag.ConversationInfo = conversation;
                }
            }
            return View(message);
        }

        public async Task<IActionResult> Reply(int id)
        {
            // get message and conversation details by id
            var message = await _context.Messages
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.MessageID == id);

            if (message == null)
            {
                return NotFound();
            }

            // create new message object
            var replyMessage = new Message
            {
                ConversationID = message.ConversationID, // serves to link new message object to the correct conversation
                SenderID = _userManager.GetUserId(User), // sets SenderID to currently logged-in user's id
                ReceiverID = message.SenderID, // replies to the original sender
                Timestamp = DateTime.Now // sets a time of now
            };

            // pass conversation info to view
            ViewBag.ConversationInfo = message.Conversation;

            return View(replyMessage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply([Bind("Content,ConversationID,ReceiverID")] Message message)
        {
            if (ModelState.IsValid)
            {
                // get currently logged-in user details
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);

                // trainer check
                var isTrainer = await _userManager.IsInRoleAsync(user, "Trainer");

                // set message fields
                message.SenderID = userId;
                message.Timestamp = DateTime.Now;
                message.IsRead = false;

                // add message to Messages db table
                _context.Messages.Add(message);

                // get conversation
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.ConversationID == message.ConversationID);

                if (conversation != null)
                {
                    // update TrainerID if it's not already set
                    // ClientID should already be set at this point so do nothing there
                    if (conversation.TrainerID == null)
                    {
                        // if currently logged-in user is of trainer usertype set Conversations' TrainerID field to user's id, otherwise set as null
                        conversation.TrainerID = isTrainer ? userId : null;
                        _context.Conversations.Update(conversation); 
                    }
                }

                // save updates and redirect to conversation thread page,and passing conversationID value info
                await _context.SaveChangesAsync();
                return RedirectToAction("Thread", new { conversationId = message.ConversationID });
            }

            // repopulate conversation info if there are validation errors to fix
            ViewBag.ConversationInfo = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationID == message.ConversationID);

            return View(message);
        }

        public async Task<IActionResult> Thread(int conversationId)
        {
            // get all messages related to conversationId
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ConversationID == conversationId)
                .OrderBy(m => m.Timestamp) // Order messages by timestamp
                .ToListAsync();

            // mark all messages in this conversation as read
            var userId = _userManager.GetUserId(User);
            foreach (var message in messages.Where(m => m.ReceiverID == userId && !m.IsRead))
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            // if there are no messages, return a NotFound notice
            if (messages == null || !messages.Any())
            {
                return NotFound();
            }

            // display messages to view
            return View(messages);
        }
    }
}    


