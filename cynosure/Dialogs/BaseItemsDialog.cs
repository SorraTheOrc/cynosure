using cynosure.Model;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace cynosure.Dialogs
{
    [Serializable]
    public abstract class BaseItemsDialog: IDialog<Standup>
    {
        protected Standup _standup;

        protected static bool IsLastInput(string input)
        {
            string[] lastWords = new string[] { "nothing", "nothing more", "nothing else", "none", "no", "no more", "finished", "done" };

            bool finished = false;
            foreach (string word in lastWords)
            {
                finished = finished || (input.ToLower() == word);
            }
            return finished;
        }

        protected static bool isAll(string input)
        {
            string[] allWords = new string[] { "all", "everything" };

            bool finished = false;
            foreach (string word in allWords)
            {
                finished = finished || (input.ToLower() == word);
            }
            return finished;
        }

        protected async Task SummaryReportAsync(IDialogContext context)
        {
            string summary = _standup.Summary();

            var text = "Your current standup report is:\n\n\n\n" + summary;
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );
            await context.PostAsync(text);
        }
        
        public abstract Task StartAsync(IDialogContext context);
    }
}