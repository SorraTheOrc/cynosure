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
            String help = "My main resonsibility is to run your standup for you.\n\n";
            help += "My commands:\n\n\n\n";
            help += "'start standup'   : Starts the standup conversation.\n\n";
            help += "'standup summary' : Displays a summary of the current standup reports";

            var reply = context.MakeMessage();
            reply.Text = help;
            reply.Speak = help;
            reply.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(reply);

            context.Done<object>(null);
        }
    }
}