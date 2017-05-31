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
    public class DoneItemsDialog : AbstractItemDialog
    {
        public override Task StartAsync(IDialogContext context)
        {
            var _standup = GetCurrentStandup(context);
            string trigger = context.Activity.AsMessageActivity().Text;
            if (trigger.Trim().ToLower().StartsWith("start"))
            {
                _standup.Done = new List<String>();
            }
            RequestInput(context);
            return Task.CompletedTask;
        }

        override protected string GetHeader(IDialogContext context)
        {
            return Standup.ItemsSummary("Items already recorded as done:", GetCurrentStandup(context).Done);
        }

        override protected string GetPromptText(IDialogContext context)
        {
            Standup standup = GetCurrentStandup(context);
            string promptText;
            if (standup.Committed.Any())
            {
                promptText = Standup.ItemsSummary("Currently committed items are:", standup.Committed);
                promptText += "Enter a new \"Done\" item or enter an existing committed item to promote it to done (you can use the numbers for this too). When you have finished updating your \"Done\" items say \"Finished\"";
            }
            else
            {
                promptText = "Enter a new \"Done\" item. When you have finished updating your \"Done\" items say \"Finished\"";
            }
            return promptText;
        }

        override protected List<String> GetItems(IDialogContext context)
        {
            return GetCurrentStandup(context).Done;
        }

        override protected async Task ProcessDialogInput(IDialogContext context, string input)
        {
            Standup standup = GetCurrentStandup(context);
            int intVal;
            if (int.TryParse(input, out intVal))
            {   
                input = standup.Committed.ElementAt(intVal - 1);
            }

            if (isAll(input))
            {
                var committed = standup.Committed;
                for (int i = standup.Committed.Count -1; i >= 0; i--)
                {
                    var item = standup.Committed.ElementAt(i);
                    standup.Done.Add(item);
                    standup.Committed.Remove(item);
                }
            }
            else
            {
                standup.Done.Add(input);
                standup.Committed.Remove(input);
            }

            context.UserData.SetValue(@"profile", standup);
            RequestInput(context);
        }

        internal override List<Command> Commands()
        {
            List<Command> commands = new List<Command>();
            commands.Add(new Command("Finished", "Finish editing the done items"));
            commands.Add(new Command("Add <item> to done.", "Add an item to the done items list.", Command.CommandType.Sentence));
            commands.Add(new Command("Remove <item> from done.", "Remove an item to the done items list and add it to the committed items.", Command.CommandType.Sentence));
            return commands;
        }

        internal override string GetCurrentDialogType()
        {
            return "completed";
        }
    }


}