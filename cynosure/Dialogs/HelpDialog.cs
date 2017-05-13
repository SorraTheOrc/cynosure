using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using cynosure.Model;
using Microsoft.Bot.Connector;

namespace cynosure.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<Standup>
    {
        public async Task StartAsync(IDialogContext context)
        {
            string title = "My main resonsibility is to run your standup for you.";
            string help = "My commands:\n\n\n\n";
            help += "'start standup'   : Starts the standup conversation.\n\n";
            help += "'standup summary' : Displays a summary of the current standup reports";

            var reply = context.MakeMessage();
            reply.Speak = title + "\n\n" + help;
            reply.InputHint = InputHints.AcceptingInput;

            reply.Attachments = new List<Attachment>
            {
                new HeroCard(title)
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.ImBack, "Start Standup", value: "start standup"),
                        new CardAction(ActionTypes.ImBack, "Standup Summary", value: "standup summary")
                    }
                }.ToAttachment()
            };

            await context.PostAsync(reply);

            context.Done<object>(null);
        }
    }
}