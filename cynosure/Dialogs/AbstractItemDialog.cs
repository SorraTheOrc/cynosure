﻿using cynosure.Model;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace cynosure.Dialogs
{
    [Serializable]
    public abstract class AbstractItemDialog: AbstractBaseDialog
    {
        /**
         * Get the text to show in the header of the main dialog.
         */
        abstract protected string GetHeader(IDialogContext context);
        
        /**
         * Get the text for the prompt in this dialog. this will be displayed after the header.
         */
        abstract protected string GetPromptText(IDialogContext context);

        /**
         * Get the items that this dialog is managing.
         */
        abstract protected List<string> GetItems(IDialogContext context);

        /**
         * Process informaiton that has been entered.
         */
        abstract protected Task ProcessDialogInput(IDialogContext context, string input);


        public override Task StartAsync(IDialogContext context)
        {
            GetCurrentStandup(context);
            RequestInput(context);
            return Task.CompletedTask;
        }

        protected Standup GetCurrentStandup(IDialogContext context)
        {
            Standup standup;
            if (!context.UserData.TryGetValue(@"profile", out standup))
            {
                standup = new Standup();
            }
            return standup;
        }

        protected void RequestInput(IDialogContext context)
        {
            List<string> items = GetItems(context);
            var promptOptions = new PromptOptions<string>(
                GetHeader(context) + "\n\n\n\n" + GetPromptText(context),
                speak: GetPromptText(context)
                );

            var prompt = new PromptDialog.PromptString(promptOptions);
            context.Call<string>(prompt, TextEnteredAsync);
        }

        protected async Task TextEnteredAsync(IDialogContext context, IAwaitable<string> result)
        {
            string input = await result;
            Standup standup = GetCurrentStandup(context);

            if (IsHelp(input))
            {
                await DisplayHelpCard(context);
                var promptOptions = new PromptOptions<string>(
                    "What do you want to do?",
                    speak: "What do you want to do?"
                    );
                var prompt = new PromptDialog.PromptString(promptOptions);
                context.Call<string>(prompt, TextEnteredAsync);
            }
            else if (IsStatus(input))
            {
                await ReportStatusAsync(context);
                RequestInput(context);
            }
            else if (IsLastInput(input))
            {
                await SummaryReportAsync(context);
                context.Done(standup);
            }
            else
            {
                await ProcessDialogInput(context, input);
            }
        }

        protected async Task ReportStatusAsync(IDialogContext context)
        {
            string header = "We are currently collecting data for \"" + GetCurrentDialogType() + "\" items.";
            await context.PostAsync(header);
        }

        protected async Task SummaryReportAsync(IDialogContext context)
        {
            Standup _standup = GetCurrentStandup(context);
            string summary = _standup.Summary();

            var text = "Your current standup report is:\n\n\n\n" + summary;
            var promptOptions = new PromptOptions<string>(
                text,
                speak: text
                );
            await context.PostAsync(text);
        }
    }
}