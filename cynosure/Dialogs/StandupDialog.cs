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
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }
            EnterDone(context);
            return Task.CompletedTask;
        }
        
        private async Task DoneCompleteCommittedAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (IsLastInput(input))
            {
                EnterDone(context);
            }
            else
            {
                _standup.Done.Add(input);
                _standup.Committed.Remove(input);
                context.UserData.SetValue(@"profile", _standup);
                EnterDone(context);
            }
        }

        Standup _standup;
        private void EnterDone(IDialogContext context)
        {
            var text = Standup.ItemsSummary("Items already recorded as done:", _standup.Done);
            string promptText;
            if (_standup.Committed.Any())
            {
                text += "\n\n\n\n" + Standup.ItemsSummary("Currently committed items are:", _standup.Committed);
                promptText = "Enter a new \"Done\" item or enter an existing committed item to promote it to done (you can use the numbers for this too). When you have finished updating your \"Done\" items say \"Finished\"";
            } else
            {
                promptText = "Enter a new \"Done\" item. When you have finished updating your \"Done\" items say \"Finished\"";
            }
            var promptOptions = new PromptOptions<string>(
                text + "\n\n\n\n" + promptText,
                speak: promptText
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, DoneItemEnteredAsync);
        }

        private async Task DoneItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (IsLastInput(input))
            {
                EnterCommitted(context);
            }
            else
            {
                int intVal;
                if (int.TryParse(input, out intVal))
                {   
                    input = _standup.Committed.ElementAt(intVal - 1);
                }

                if (isAll(input))
                {
                    var committed = _standup.Committed;
                    for (int i = _standup.Committed.Count -1; i >= 0; i--)
                    {
                        var item = _standup.Committed.ElementAt(i);
                        _standup.Done.Add(item);
                        _standup.Committed.Remove(item);
                    }
                }
                else
                {
                    _standup.Done.Add(input);
                    _standup.Committed.Remove(input);
                }

                context.UserData.SetValue(@"profile", _standup);
                EnterDone(context);
            }
        }

        private void EnterCommitted(IDialogContext context)
        {
            var text = Standup.ItemsSummary("Items already recorded as focus items for today:", _standup.Committed);
            string promptText;
            if (_standup.Committed.Any())
            {
                promptText = "What else are you focusing on today?";
            }
            else
            {
                promptText = "What will you focus on today?";
            }
            var promptOptions = new PromptOptions<string>(
                text + "\n\n\n\n" + promptText,
                speak: promptText
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, CommittedItemEnteredAsync);
        }

        private async Task CommittedItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (IsLastInput(input))
            {
                EnterIssues(context);
            }
            else
            {
                _standup.Committed.Add(input);
                context.UserData.SetValue(@"profile", _standup);
                EnterCommitted(context);
            }
        }

        private void EnterIssues(IDialogContext context)
        {
            var text = Standup.ItemsSummary("Items already recorded as blocking:", _standup.Issues);
            string promptText;
            if (_standup.Issues.Any())
            {
                promptText = "What other blockers you are facing right now?";
            }
            else
            {
                promptText = "What blockers are you facing at the moment?";
            }
            var promptOptions = new PromptOptions<string>(
                text + "\n\n\n\n" + promptText,
                speak: promptText
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, IssuesItemEnteredAsync);
        }

        private async Task IssuesItemEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (IsLastInput(input))
            {
                await SummaryReportAsync(context);
            }
            else
            {
                _standup.Issues.Add(input);
                context.UserData.SetValue(@"profile", _standup);
                EnterIssues(context);
            }
        }

        private static bool IsLastInput(string input)
        {
            string[] lastWords = new string[] { "nothing", "nothing more", "nothing else", "none", "no", "no more", "finished", "done" };

            bool finished = false;
            foreach (string word in lastWords)
            {
                finished = finished || (input.ToLower() == word);
            }
            return finished;
        }

        private static bool isAll(string input)
        {
            string[] allWords = new string[] { "all", "everything"};

            bool finished = false;
            foreach (string word in allWords)
            {
                finished = finished || (input.ToLower() == word);
            }
            return finished;
        }

        private async Task SummaryReportAsync(IDialogContext context)
        {
            string summary = _standup.Summary();

            var text = "Your current standup report is:\n\n\n\n" + summary;
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );
            await context.PostAsync(text);
            context.Done(_standup);
        }
    }
}