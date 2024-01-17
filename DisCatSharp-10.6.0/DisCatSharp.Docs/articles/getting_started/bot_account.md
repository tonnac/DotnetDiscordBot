---
uid: getting_started_bot_account
title: Creating a Bot Account
author: DisCatSharp Team
---

# Creating a Bot Account

## Create an Application

> [!NOTE]
> You can also use discords official [getting started](https://discord.com/developers/docs/getting-started#building-your-first-discord-app) tutorial till step 2.

Before you're able to create a [bot account](https://discord.com/developers/docs/topics/oauth2#bots) to interact with the Discord API, you'll need to create a new OAuth2 application.
[Create](https://discord.com/developers/applications?new_application=true) a new application in the [Discord Developer Portal](https://discord.com/developers/).

![Discord Developer Portal](/images/getting_started_bot_account_01.png)

<br/>
You'll then be prompted to enter a name for your application.<br/>

![Naming Application](/images/getting_started_bot_account_02.png "Naming Application")

The name of your application will be the name displayed to users when they add your bot to their Discord server.<br/>
With that in mind, it would be a good idea for your application name to match the desired name of your bot.

Enter your desired application name into the text box, accept the [Developer Terms of Service](https://discord.com/developers/docs/policies-and-agreements/terms-of-service) and [Developer Policy](https://discord.com/developers/docs/policies-and-agreements/developer-policy) and hit the `Create` button.

After you hit `Create`, you'll be taken to the application page for your newly created application.

![Application Page](/images/getting_started_bot_account_03.png)

That was easy, wasn't it?

Before you move on, you may want to upload an icon for your application and provide a short description of what your bot will do.
As with the name of your application, the application icon and description will be displayed to users when adding your bot.

# Using Your Bot Account

## Invite Your Bot

Now that you have a bot account, you'll probably want to invite it to a server!

A bot account joins a server through a special invite link that'll take users through the OAuth2 flow;
you'll probably be familiar with this if you've ever added a public Discord bot to a server.
To get the invite link for your bot, head on over to the `OAuth2` page of your application and select the `URL Generator` page.

![OAuth2 URL Generator](/images/getting_started_bot_account_06.png "OAuth2 URL Generator")

<br/>
We'll be using the *OAuth2 URL Generator* on this page.<br/>
Simply tick `bot` under the *scopes* panel; your bot invite link will be generated directly below.

![OAuth2 Scopes](/images/getting_started_bot_account_07.png "OAuth2 Scopes")

<br/>
By default, the generated link will not grant any permissions to your bot when it joins a new server.<br/>
If your bot requires specific permissions to function, you'd select them in the *bot permissions* panel.

![Permissions](/images/getting_started_bot_account_08.png "Permissions Panel")

The invite link in the _scopes_ panel will update each time you change the permissions.<br/>
Be sure to copy it again after any changes!

## Get Bot Token

Instead of logging in to Discord with a username and password, bot accounts use a long string called a _token_ to authenticate.
You'll want to retrieve the token for your bot account so you can use it with DisCatSharp.

Head back to the bot page and click on `Reset Token` just below the bot's username field.

![Token Reset](/images/getting_started_bot_account_09.png "Token Reset")

Click on `Yes, do it!` to confirm the reset.

![Token Reset Confirm](/images/getting_started_bot_account_10.png "Token Reset Confirmation")

Go ahead and copy your bot token and save it somewhere. You'll be using it soon!

> [!IMPORTANT]
> Handle your bot token with care! Anyone who has your token will have access to your bot account.
> Be sure to store it in a secure location and _never_ give it to _anybody_.
>
> If you ever believe your token has been compromised, be sure to hit the `Reset Token` button (as seen above) to invalidate your old token and get a brand new token.

## Write Some Code

You've got a bot account set up and a token ready for use.<br/>
Sounds like it's time for you to [write your first bot](xref:getting_started_first_bot)!
