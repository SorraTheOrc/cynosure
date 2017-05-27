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
    public class DoneItemsDialog : BaseItemsDialog
    {
        public override Task StartAsync(IDialogContext context)
        {
            if (!context.UserData.TryGetValue(@"profile", out _standup))
            {
                _standup = new Standup();
            }
            string trigger = context.Activity.AsMessageActivity().Text;
            if (trigger.Trim().ToLower().StartsWith("start"))
            {
                _standup.Done = new List<String>();
            }
            EnterDone(context);
            return Task.CompletedTask;
        }
        
        private async Task DoneCompleteCommittedAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            if (IsLastInput(input))
            {
                await SummaryReportAsync(context);
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
                await SummaryReportAsync(context);
                context.Done(_standup);
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
    }
}