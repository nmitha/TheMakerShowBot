using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Cognitive.LUIS;
using System.Diagnostics;
using Microsoft.Bot.Builder.Dialogs;
using HelpBot.FormFlows;
using Microsoft.Bot.Builder.FormFlow;

namespace HelpBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        LuisClient luisClient = new LuisClient(
                        /*appid:*/ "36ce03d4-3cd6-49f7-a847-88746b4120dc",
                        /*appkey:*/ "ae4c72bef69a477e898d3cad9b57714e"
                        );

        //LuisClient cortanaLuisClient = new LuisClient(
        //    "c413b2ef-382c-45bd-8ff0-f76d60e2a821",
        //    "ae4c72bef69a477e898d3cad9b57714e"
        //    );

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (activity.Type != ActivityTypes.Message)
            {
                HandleSystemMessage(activity);
            }
            else
            {
                string msg = activity.Text;

                // Parse the user's meaning (intent) via Language Understanding (LUIS) in Cognitive Services
                string replyMsg = await GetReplyMessage(msg);

                bool isConversationAboutAddingPrinter = BotState.ConversationsInPrinterDialog.Contains(activity.Conversation.Id);
                if (isConversationAboutAddingPrinter || replyMsg == "[addprinter]") // quick hack for this demo and special case
                {
                    if (!isConversationAboutAddingPrinter)
                    {
                        BotState.ConversationsInPrinterDialog.Add(activity.Conversation.Id); // it is now
                    }
                    await Microsoft.Bot.Builder.Dialogs.Conversation.SendAsync(activity, MakeRootAddPrinterDialog);
                }
                else
                {
                    Activity reply = activity.CreateReply(replyMsg);

                    if (replyMsg.Contains("restart"))
                    {
                        reply.Attachments.Add(new Attachment()
                        {
                            ContentUrl = "http://cdn.makeuseof.com/wp-content/uploads/2015/07/Windows-10-Restart.png",
                            ContentType = "image/png",
                            Name = "Rebooting in Windows 10"
                        });
                    }                   

                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<string> GetReplyMessage(string msg)
        {
            string defaultReplyMsg = "I'm not so good at chit chat, but tell me about the IT support you need and I can help!";
            string replyMsg = defaultReplyMsg;
            var luisResult = await luisClient.Predict(msg);
            string intent = luisResult.GetTopIntentName();

            // Not using a switch statement because I might want to add more conditions (e.g. check intent and also check entities or something else)
            if (string.IsNullOrEmpty(intent) || intent == "None")
            {
                // Try fall back to Cortana if we don't know what to do with this message:
                //var cortanaResult = await cortanaLuisClient.Predict(msg);
                //if (! cortanaResult.HasIntent())
                //{
                //    return defaultReplyMsg;
                //}
                //replyMsg = $"(Cortana) Intent: {cortanaResult.TopScoringIntent.Name}";
                //string dialogPrompt = string.Empty;
                //if (cortanaResult.isAwaitingDialogResponse() && cortanaResult.DialogResponse != null && !string.IsNullOrEmpty(cortanaResult.DialogResponse.Prompt))
                //{
                //    replyMsg += $"\r\nPrompt: {cortanaResult.DialogResponse.Prompt}";
                //}

                return defaultReplyMsg;
            }
            else if (intent == "thank you")
            {
                replyMsg = "You're welcome. Let me know if there's anything else.";
            }
            else if (intent == "computer slow")
            {
                replyMsg = "If you're computer is slow, maybe you should try to restart it.\r\n\r\nIt seems to me like you're using Windows 10, so here's how you'd restart: ";
            }
            else if (intent == "add a printer")
            {
                replyMsg = "[addprinter]"; //this is a hack to indicate we should enter the add printer dialog.  previous: "I can help you add a printer.  First, are you using Windows XP, Windows 7 or a Mac?";
            }
            else if (intent == "access to system")
            {
                string system = MapSystemNameToFriendlyName(luisResult.GetEntityValue("system"));
                replyMsg = $"It sounds like you need access to {system}.  That should be no problem."
                    + $"\r\n\r\nIn order to go ahead and do that, I just need your manager Katie Jordan to chat with me about it (or tell her to email the details to me at helpbot@cloud.com).  Once she confirms you're authorized for that I should be able to set you up with access!"
                    + $"\r\n\r\nI've gone ahead and created ticket for your system access request, the ticket # is {DateTime.Now.ToString("MMddHHmmss")}";
            }
            else
            {
                replyMsg = $"I think what you're saying has to do with '{intent}' but I'm not programmed to help with that yet.  Sorry!";
            }

            return replyMsg;
        }

        internal static string MapSystemNameToFriendlyName(string system)
        {
            if (string.IsNullOrEmpty(system))
            {
                return "an application or system we manage";
            }
            else if (system.Equals("sharepoint", StringComparison.OrdinalIgnoreCase))
            {
                return "a SharePoint site";
            }
            else
            {
                return system.ToUpper();
            }
        }

        internal static IDialog<AddPrinterInfo> MakeRootAddPrinterDialog()
        {
            return Chain.From(() => FormDialog.FromForm(AddPrinterInfo.BuildForm));
        }

        private Activity HandleSystemMessage(Activity activity)
        {
            if (activity.Type == ActivityTypes.Ping)
            {
                // Check if service is alive
            }
            else if (activity.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (activity.Type == ActivityTypes.Typing)
            {
                // Lets your bot indicate whether the user or bot is typing
            }

            return null;
        }
    }
}