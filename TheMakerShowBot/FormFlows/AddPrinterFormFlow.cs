using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
#pragma warning disable 649
// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace HelpBot.FormFlows
{
    public enum ComputerName
    {
        [Describe(Description = "Desktop W01573398"), Terms("Desktop W01573398", "W01573398", "W 01573398", "desktop")]
        W01573398,
        [Describe(Description = "Laptop L03848491"), Terms("Laptop L03848491", "L03848491", "L 03848491", "laptop")]
        L03848491,
        [Terms("Add Printer To All", ".*all.*", ".*both.*")]
        AddPrinterToAll
    };
    
    [Serializable]
    public class AddPrinterInfo
    {
        [Prompt("What's the printer's machine name? (you'll find it clearly labelled on a sticker on the printer itself)")]
        public string PrinterName;

        [Prompt("My records indicate that you have 2 computers.  Which should we add the printer to?  (If you're not sure which is which, then add printer to all!)  {||}")]
        public ComputerName? ComputersToAddPrinterTo;

        public static IForm<AddPrinterInfo> BuildForm()
        {
            OnCompletionAsyncDelegate<AddPrinterInfo> processOrder = async (context, state) =>
            {
                var msg = context.MakeMessage();
                msg.Text = $"Done! You should be able to print there within a few moments.";
                await context.PostAsync(msg);
            };

            return new FormBuilder<AddPrinterInfo>()
                    .Message("Let's get that printer added to your computer for you.  I just need some more information.")
                    //.Message(new MessageDelegate())
                    .OnCompletion(async (context, state) =>
                    {
                        var msg = context.MakeMessage();
                        msg.Text = $"Done! You should see the printer named \"{state.PrinterName}\" the next time you print.";
                        await context.PostAsync(msg);

                        //TODO: Fix this
                        BotState.ConversationsInPrinterDialog.Clear();
                    })
                    .Build();
        }
    };
}