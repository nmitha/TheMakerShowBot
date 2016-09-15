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

namespace HelpBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        LuisClient luisClient = new Microsoft.Cognitive.LUIS.LuisClient(
                        /*appid:*/ "36ce03d4-3cd6-49f7-a847-88746b4120dc",
                        /*appkey:*/ "ae4c72bef69a477e898d3cad9b57714e"
                        );

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
                // Parse the user's meaning via Language Understanding (LUIS) in Cognitive Services
                //MakerLUIS mkLuis = await LUISMakerShowClient.ParseUserInput(activity.Text);
                string msg = activity.Text;
                string replyMsg = string.Empty;
                
                var luisResult = await luisClient.Predict(msg);
                string intent = luisResult.TopScoringIntent.Name;

                // Not using a switch because I might want to add more conditions (e.g. check intent and also check entities or something else)
                if (string.IsNullOrEmpty(intent) || intent == "None")
                {
                    replyMsg = "I'm not so good at chit chat, but tell me about the IT support you need and I can help!";
                }
                else if (intent == "thank you")
                {
                    replyMsg = "You're welcome. Let me know if there's anything else.";
                }
                else if (intent == "computer slow")
                {
                    replyMsg = "If you're computer is slow, maybe you should try to reboot";
                }
                else if (intent == "add a printer")
                {
                    replyMsg = "I can help you add a printer.  First, are you using Windows XP, Windows 7 or a Mac?";
                }

                Activity reply = activity.CreateReply(replyMsg);
                //reply.Attachments.Add(new Attachment(contentUrl: ""))

                ///connector.Conversations.ReplyToActivity()

                await connector.Conversations.ReplyToActivityAsync(reply);

                // After replying as soon as possible, log the LUIS result to the console:
                System.Diagnostics.Trace.WriteLine(JsonConvert.SerializeObject(luisResult));
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
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