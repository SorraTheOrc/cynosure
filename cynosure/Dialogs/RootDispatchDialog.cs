using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using cynosure.Model;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.ApplicationInsights;
using System.Reflection;
using System.Collections.Generic;

namespace cynosure.Dialogs
{
    [Serializable]
    public class RootDispatchDialog : DispatchDialog
    {

        Standup _standup;

        [MethodBind]
        [ScorableGroup(0)]
        private async Task ActivityHandler(IDialogContext context, IActivity activity)
        { 
            try
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        this.ContinueWithNextGroup();
                        break;
                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    case ActivityTypes.Ping:
                    default:
                        break;
                }
            }
            catch (Microsoft.Bot.Builder.Internals.Fibers.InvalidNeedException ex)
            {
                var telemetry = new TelemetryClient();
                telemetry.TrackException(ex);
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = ((Activity)activity).CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Let's start over");
                await connector.Conversations.ReplyToActivityAsync(reply);
                StateClient stateClient = activity.GetStateClient();
                await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
            }
        }

        private async Task standupUpdatedAsync(IDialogContext context, IAwaitable<Standup> result)
        {
            var standup = await result;
            context.UserData.SetValue(@"standup", standup);
        }

        private async Task profileUpdated(IDialogContext context, IAwaitable<UserProfile> result)
        {
            var profile = await result;
            context.UserData.SetValue(@"profile", profile);
        }

        [RegexPattern("start standup|standup|start|stand up")]
        [ScorableGroup(1)]
        public void StartStandup(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Start Standup");
            context.Call<Standup>(new DoneItemsDialog(), standupUpdatedAsync);
        }

        [RegexPattern("edit done|done|edit completed|completed|edite complete|complete")]
        [ScorableGroup(1)]
        public void EditDone(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Edit Done");
            context.Call<Standup>(new DoneItemsDialog(), standupUpdatedAsync);
        }

        [RegexPattern("^Add (?<item>.*) to done items.")]
        [RegexPattern("^Add (?<item>.*) to done.")]
        [ScorableGroup(1)]
        public void AddDone(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Done.Add(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Added \"" + itemText + "\" to done items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("^Remove (?<item>.*) from done.")]
        [RegexPattern("^Remove (?<item>.*) from done items.")]
        [ScorableGroup(1)]
        public void RemoveDone(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Done.Remove(itemText);
            standup.Committed.Add(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Moved \"" + itemText + "\" from done to committed items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("edit committed|committed|edit commitments|commitments")]
        [ScorableGroup(1)]
        public void EditCommitments(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Edit Commitments");
            context.Call<Standup>(new CommittedItemsDialog(), standupUpdatedAsync);
        }

        [RegexPattern("^Add (?<item>.*) to committed items.")]
        [RegexPattern("^Add (?<item>.*) to committed.")]
        [RegexPattern("^Add item for today saying (?<item>.*)")]
        [ScorableGroup(1)]
        public void AddCommitted(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Committed.Add(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Added \"" + itemText + "\" to comitted items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("^Remove (?<item>.*) from committed.")]
        [ScorableGroup(1)]
        public void RemoveCommitted(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Committed.Remove(itemText);
            standup.Backlog.Add(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Moved \"" + itemText + "\" from commited to backlog items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("^Add (?<item>.*) to backlog items.")]
        [RegexPattern("^Add (?<item>.*) to backlog.")]
        [ScorableGroup(1)]
        public void AddBacklog(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Backlog.Add(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Added \"" + itemText + "\" to backlog items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("^Remove (?<item>.*) from backlog.")]
        [ScorableGroup(1)]
        public void RemoveBacklog(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Backlog.Remove(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Removed \"" + itemText + "\" from backlog items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("^Delete (?<item>.*) from (?<list>.*).")]
        [ScorableGroup(1)]
        public void Delete(IDialogContext context, IActivity activity, [Entity("item")] string itemText, [Entity("list")] string list)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            if (list.ToLower().Equals("done"))
            {
                standup.Done.Remove(itemText);
            }
            else if (list.ToLower().Equals("committed"))
            {
                standup.Committed.Remove(itemText);
            }
            else if (list.ToLower().Equals("barriers"))
            {
                standup.Issues.Remove(itemText);
            }
            else if (list.ToLower().Equals("backlog"))
            {
                standup.Backlog.Remove(itemText);
            }
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Removed \"" + itemText + "\" from " + list + " items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("edit issues|issues|edit barriers|barriers|edit needs|needs|edit blockers|blockers")]
        [ScorableGroup(1)]
        public void EditIssues(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Edit Issues");
            context.Call<Standup>(new IssueItemsDialog(), standupUpdatedAsync);
        }

        [RegexPattern("^Add (?<item>.*) to barriers.")]
        [RegexPattern("^Add (?<item>.*) to needs.")]
        [RegexPattern("^Add a need for (?<item>.*).")]
        [ScorableGroup(1)]
        public void AddBarrier(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Issues.Add(itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Added \"" + itemText + "\" to blocking items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("^Remove (?<item>.*) from barriers.")]
        [RegexPattern("^Remove (?<item>.*) from needs.")]
        [RegexPattern("^Remove need for (?<item>.*).")]
        [ScorableGroup(1)]
        public void RemoveBarrier(IDialogContext context, IActivity activity, [Entity("item")] string itemText)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                context.PostAsync("Not currently in a standup. Use \"start standup\" to get started.");
            }
            standup.Issues.Remove(itemText);
            standup.Done.Add("Removed need: " + itemText);
            context.UserData.SetValue(@"profile", standup);

            string prompt = "Removed \"" + itemText + "\" from done items and added to committed items.";
            prompt += "\n\n\n\n" + standup.Summary();
            context.PostAsync(prompt);
        }

        [RegexPattern("standup summary|summary|standup report|report")]
        [ScorableGroup(1)]
        public async Task StandupSummary(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Summarize Standup");
            if (context.UserData.TryGetValue(@"standup", out _standup))
            {
                await context.PostAsync(_standup.Summary());
            }
            else
            {
                await context.PostAsync("There is no standup data right now. You can 'start standup' if you like");
            }
            context.Done(true);
        }

        [RegexPattern("help")]
        [ScorableGroup(2)]
        public async Task Help(IDialogContext context, IActivity activity)
        {
            await this.DefaultAsync(context, activity);
        }

        [RegexPattern("version")]
        [ScorableGroup(2)]
        public async Task Version(IDialogContext context, IActivity activity)
        {
            Assembly thisAssem = typeof(RootDispatchDialog).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();

            await context.PostAsync(thisAssemName.ToString());
        }

        [MethodBind]
        [ScorableGroup(2)]
        public async Task DefaultAsync(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            try
            {
                telemetry.TrackEvent("Display Help");
                context.Call(new HelpDialog(), AfterDialog);
            }
            catch (Microsoft.Bot.Builder.Internals.Fibers.InvalidNeedException ex)
            {
                telemetry.TrackException(ex);
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = ((Activity)activity).CreateReply("Sorry, I'm having some difficulties here. I have to reboot myself. Let's start over.");
                await connector.Conversations.ReplyToActivityAsync(reply);
                StateClient stateClient = activity.GetStateClient();
                await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                telemetry.TrackEvent("Cleared users state.");
            }
        }

        [RegexPattern("hello|hi")]
        [ScorableGroup(1)]
        public async Task Hello(IDialogContext context, IActivity activity)
        {
            var telemetry = new TelemetryClient();
            telemetry.TrackEvent("Say Hi");
            await context.PostAsync(@"Hello, I'm Cynosure. Say 'help' to learn more about what I can do.");
            context.Done(true);
        }
        
        private static async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
        
    }
}