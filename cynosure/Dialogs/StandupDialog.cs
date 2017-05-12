using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using cynosure.Model;

namespace cynosure.Dialogs
{
    [Serializable]
    public class StandupDialog : IDialog<Standup>
    {
        public Task StartAsync(IDialogContext context)
        {
            EnsureStandupReportCompleted(context);
            return Task.CompletedTask;
        }

        Standup _standup;
        private void EnsureStandupReportCompleted(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }

            PromptDialog.Text(context, DoneItemEnteredAsync, "What did you complete in the last cycle?");
        }

        private async Task DoneItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (input.ToLower() == "no")
            {
                EnsureCommitted(context);
            } else { 
                _standup.Done.Add(input);
                PromptDialog.Text(context, DoneItemEnteredAsync, "Did you complete anything else in the last cycle?");
            }
        }
        private void EnsureCommitted(IDialogContext context)
        {
            PromptDialog.Text(context, CommittedItemEnteredAsync, "What are you focussing on now?");
        }

        private async Task CommittedItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (input.ToLower() == "no")
            {
                EnsureIssues(context);
            }
            else
            {
                _standup.Committed.Add(input);
                PromptDialog.Text(context, CommittedItemEnteredAsync, "Do you have any other focus items right now?");
            }
        }
        private void EnsureIssues(IDialogContext context)
        {
            PromptDialog.Text(context, IssuesItemEnteredAsync, "Are there any issues blocking you right now?");
        }

        private async Task IssuesItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (input.ToLower() == "no" || input.ToLower() == "none" || input.ToLower() == "nothing" )
            {
                SummaryReportAsync(context);
            }
            else
            {
                _standup.Issues.Add(input);
                PromptDialog.Text(context, IssuesItemEnteredAsync, "Any other blockers right now?");
            }            
        }

        private void SummaryReportAsync(IDialogContext context)
        {
            String summary = "DONE:\n\n";
            foreach (var item in _standup.Done)
                summary += item + "\n\n";
            summary += "\n\n\n\nFOCUSING ON:\n\n";
            foreach (var item in _standup.Committed)
                summary += item + "\n\n";
            summary += "\n\n\n\nBARRIERS:\n\n";
            foreach (var item in _standup.Issues)
                summary += item + "\n\n";
            summary += "\n\n\n\nDo you want to post this standup summary?";
            PromptDialog.Text(context, StandupCompleteAsync, summary);
        }

        private async Task StandupCompleteAsync(IDialogContext context, IAwaitable<string> result)
        {
            String input = await result;
            if (input.ToLower() == "yes")
            {
                context.Done(_standup);
            }
            else
            {
                EnsureStandupReportCompleted(context);
            }
        }
    }
}