using System.Globalization;
using System.Text.Json.Nodes;
using CmdPalPomodoro.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalPomodoro.Pages;

internal sealed partial class PomodoroSettingsPage : ContentPage
{
    private readonly PomodoroSettingsForm _form = new();

    public PomodoroSettingsPage()
    {
        Id = "com.github.cmdpal.pomodoro.settings";
        Name = "Settings";
        Title = "Pomodoro settings";
        Icon = new IconInfo("\uE713");
    }

    public override IContent[] GetContent()
    {
        _form.Refresh();
        return [_form];
    }
}

internal sealed partial class PomodoroSettingsForm : FormContent
{
    public PomodoroSettingsForm()
    {
        Refresh();
    }

    public void Refresh()
    {
        var settings = PomodoroTimerService.Instance.Settings;
        TemplateJson = $$"""
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.6",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Pomodoro",
                    "size": "medium",
                    "weight": "bolder",
                    "wrap": true
                },
                {
                    "type": "Input.Number",
                    "id": "WorkMinutes",
                    "label": "Focus (minutes)",
                    "value": {{settings.WorkMinutes}},
                    "min": 1,
                    "max": 180,
                    "isRequired": true,
                    "errorMessage": "Enter 1 to 180"
                },
                {
                    "type": "Input.Number",
                    "id": "ShortBreakMinutes",
                    "label": "Short break (minutes)",
                    "value": {{settings.ShortBreakMinutes}},
                    "min": 1,
                    "max": 60,
                    "isRequired": true,
                    "errorMessage": "Enter 1 to 60"
                },
                {
                    "type": "Input.Number",
                    "id": "LongBreakMinutes",
                    "label": "Long break (minutes)",
                    "value": {{settings.LongBreakMinutes}},
                    "min": 1,
                    "max": 120,
                    "isRequired": true,
                    "errorMessage": "Enter 1 to 120"
                },
                {
                    "type": "Input.Number",
                    "id": "SessionsUntilLongBreak",
                    "label": "Focus sessions before a long break",
                    "value": {{settings.SessionsUntilLongBreak}},
                    "min": 1,
                    "max": 12,
                    "isRequired": true,
                    "errorMessage": "Enter 1 to 12"
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Save"
                }
            ]
        }
        """;
    }

    public override CommandResult SubmitForm(string inputs, string data)
    {
        var formInput = JsonNode.Parse(inputs)?.AsObject();
        if (formInput is null)
        {
            return CommandResult.ShowToast("Could not read the form.");
        }

        var candidate = new PomodoroSettings
        {
            WorkMinutes = ReadInt(formInput, "WorkMinutes"),
            ShortBreakMinutes = ReadInt(formInput, "ShortBreakMinutes"),
            LongBreakMinutes = ReadInt(formInput, "LongBreakMinutes"),
            SessionsUntilLongBreak = ReadInt(formInput, "SessionsUntilLongBreak"),
        };

        if (!PomodoroTimerService.Instance.TryUpdateSettings(candidate, out var error))
        {
            return CommandResult.ShowToast(new ToastArgs
            {
                Message = error,
                Result = CommandResult.KeepOpen(),
            });
        }

        Refresh();
        return CommandResult.ShowToast(new ToastArgs
        {
            Message = "Settings saved",
            Result = CommandResult.KeepOpen(),
        });
    }

    private static int ReadInt(JsonObject input, string name)
    {
        var node = input[name];
        if (node is null)
        {
            return 0;
        }

        if (node is JsonValue value && value.TryGetValue<int>(out var number))
        {
            return number;
        }

        return int.TryParse(node.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }
}
