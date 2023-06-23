using DisCatSharp;
using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DiscordBot.Yacht;

enum EYachtPointType : ushort 
{
    Aces,
    Deuces,
    Threes,
    Fours,
    Fives,
    Sixes,
    Choice,
    FourOfaKind,
    FullHouse,
    SStraight,
    LStraight,
    Yacht,
    SubTotal,
    Bonus,
    Total,
}

public class YachtGame
{
    private readonly string[] _enNums = new string[] {"Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"};
    public DiscordUser? _1P;
    public DiscordUser? _2P;
    public DiscordThreadChannel? _yachtChannel;
    private DiscordMessage? _yachtScoreUiMessage;
    private DiscordMessage? _yachtDiceTrayUiMessage;

    private int _turn = 1;
    private int _diceChance = 3;

    private readonly int[,] _points = new int[2, 13];
    private readonly int[] _tempPoints = new int[12];
    private readonly int[] _dices = new int[5];

    private readonly bool[] _diceTarget = new bool[5];


    public DiscordUser? CurrPlayer => _turn % 2 == 0 ? _2P : _1P;
    private string TurnName => _turn % 2 == 0 ? "2P" : "1P";
    public async Task<bool> SetUi(DiscordClient discordClient)
    {
        if (_yachtDiceTrayUiMessage == null && _yachtScoreUiMessage == null)
        {
            _yachtScoreUiMessage = await _yachtChannel?.SendMessageAsync(ScoreBoardVisualize(discordClient))!;
            _yachtDiceTrayUiMessage = await _yachtChannel?.SendMessageAsync(DiceTrayVisualize(discordClient!))!; 
            
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("1️⃣"));
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("2️⃣"));
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("3️⃣"));
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("4️⃣"));
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("5️⃣"));
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🔄"));
            return false;
        }
        return true;
    }
    private void PointTempSettle()
    {
        for (int i = 0; i < _tempPoints.Length; i++)
        {
            _tempPoints[i] = 0;
        }

        int choice = 0;

        foreach (var dice in _dices)
        {
            choice += dice;
        }

        _tempPoints[(int)EYachtPointType.Choice] = choice;

        int[] fullHouseCheck = new int[6];

        for (int i = 0; i < 6; i++)
        {
            int currCheckDiceNum = i + 1;
            int count = 0;
            for (int j = 0; j < 5; j++)
            {
                if (_dices[j] == currCheckDiceNum)
                    count++;
            }
            fullHouseCheck[i] = count;

            _tempPoints[i] = count * (i + 1);

            if (count >= 4)
            {
                _tempPoints[(int) EYachtPointType.FourOfaKind] = choice;
            }

            if (count >= 5)
            {
                _tempPoints[(int) EYachtPointType.Yacht] = 50;
                _tempPoints[(int) EYachtPointType.FullHouse] = choice;
            }

        }

        bool two = false;
        bool three = false;

        for (int i = 0; i < 6; i++)
        {
            if (fullHouseCheck[i] == 3)
                three = true;

            if (fullHouseCheck[i] == 2)
                two = true;
        }

        if (two && three)
        {
            _tempPoints[(int)EYachtPointType.FullHouse] = choice;
        }

        List<int> sortList = new List<int>();

        sortList.AddRange(_dices);
        sortList.Sort();

        int straightCheck = 0;

        for (int i = 0; i < 4; i++)
        {
            if (sortList[i] + 1 != sortList[i + 1])
            {
                straightCheck++;
            }

        }

        if (straightCheck == 0)
            _tempPoints[(int)EYachtPointType.LStraight] = 30;

        for (int i = 0; i < 3; i++)
        {
            int count = 0;
            for (int j = 0; j < 4; j++)
            {
                if (fullHouseCheck[j + i] > 0)
                    count++;
            }

            if (count >= 4)
                _tempPoints[(int)EYachtPointType.SStraight] = 15;

        }
    }
    private void PointSettle()
    {
        int topSum = 0;
        int turn = _turn % 2;
        for (int i = 0; i < 6; i++)
        {
            topSum += _points[turn,i];
        }

        _points[turn,(int)EYachtPointType.SubTotal] = topSum;
        if (topSum >= 63)
        {
            _points[turn,(int)EYachtPointType.Bonus] = 35;
        }
        int totalSum = 0;
        for (int i = 0; i < 13; i++)
        {
            totalSum += _points[turn,i];
        }
        _points[turn,(int)EYachtPointType.Total] = totalSum;
       
    }
    public void GameSettle()
    {

    }
    public async Task RefreshGameBoard(DiscordClient discordClient)
    {
        if (!await SetUi(discordClient))
        {
            return;
        }
        Optional<DiscordEmbed> diceTrayEmbed = new Optional<DiscordEmbed>(DiceTrayVisualize(discordClient));
        await _yachtDiceTrayUiMessage?.ModifyAsync(diceTrayEmbed)!;
        Optional<DiscordEmbed> scoreEmbed = new Optional<DiscordEmbed>(ScoreBoardVisualize(discordClient));
        await _yachtScoreUiMessage?.ModifyAsync(scoreEmbed)!;
    }
    public async Task DiceTrayMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtDiceTrayUiMessage!.Id)
            return;
            
        if (CurrPlayer?.Id != eventArgs.User.Id)
            return;
        
        switch (eventArgs.Emoji.Name)
        {
            case "1️⃣": 
                _diceTarget[0] = true; 
                break;
            case "2️⃣": 
                _diceTarget[1] = true; 
                break;
            case "3️⃣": 
                _diceTarget[2] = true; 
                break;
            case "4️⃣": 
                _diceTarget[3] = true; 
                break;
            case "5️⃣": 
                _diceTarget[4] = true; 
                break;
            case "6️⃣": 
                _diceTarget[5] = true;  
                break;
            case "🔄":
                DiceRoll(true);
                await RefreshGameBoard(client);
                await eventArgs.Message.DeleteReactionsEmojiAsync(eventArgs.Emoji);
                if (_diceChance > 0)
                    await eventArgs.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🔄"));
                break;
        }
    }

    public async Task DiceTrayMessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;

        if (eventArgs.Message.Id != _yachtDiceTrayUiMessage!.Id)
            return;

        if (CurrPlayer?.Id != eventArgs.Message.Author.Id)
            return;

        switch (eventArgs.Emoji.Name)
        {
            case "1️⃣":
                _diceTarget[0] = false;
                break;
            case "2️⃣":
                _diceTarget[1] = false;
                break;
            case "3️⃣":
                _diceTarget[2] = false;
                break;
            case "4️⃣":
                _diceTarget[3] = false;
                break;
            case "5️⃣":
                _diceTarget[4] = false;
                break;
            case "6️⃣":
                _diceTarget[5] = false;
                break;
        }
    }

    public async Task ScoreBoardMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtScoreUiMessage!.Id)
            return;

        if (CurrPlayer?.Id != eventArgs.Message.Author.Id)
            return;


    }
    public async Task ScoreBoardMessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtScoreUiMessage!.Id)
            return;

        if (CurrPlayer?.Id != eventArgs.Message.Author.Id)
            return;

    }

    void DiceRoll(bool forceRoll)
    {
        if (_diceChance <= 0)
            return;
        
        Random rand = new Random();
        for (int i = 0 ; i<_dices.Length;i++)
        {
            if (!_diceTarget[i] && 0 < _diceChance && !forceRoll)
                continue;
            _dices[i] = rand.Next(1, 7);
        }

        for (int i = 0; i < _diceTarget.Length; i++)
        {
            _diceTarget[i] = false;
        }

        _diceChance--;
    }
    

    void DiceReset()
    {
        _diceChance = 3;
        for (int i = 0; i < _dices.Length; i++)
        {
            _dices[i] = 0;
        }
    }

    private DiscordEmbedBuilder ScoreBoardVisualize(DiscordClient discordClient)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .WithTitle("ScoreBoard")
            .AddField(new DiscordEmbedField("ROUNDS", $"{_turn / 2 + (_turn % 2 == 1 ? 1 : 0)}", true))
            .AddField(new DiscordEmbedField("1P", $"{_1P?.DisplayName}", true))
            .AddField(new DiscordEmbedField("2P", $"{_2P?.DisplayName}", true));


        DiscordEmbedField discordEmbedField = new DiscordEmbedField("","",false);

        return embedBuilder;
    }

    private DiscordEmbedBuilder DiceTrayVisualize(DiscordClient discordClient)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Green)
            .WithTitle($"{TurnName} : {CurrPlayer?.Username}")
            .AddField(new DiscordEmbedField("ReRoll Count", $"{_diceChance}",true));

        DiscordEmbedField discordEmbedField = new DiscordEmbedField("","",true);
        for (int i = 0; i < _dices.Length; i++)
        {
            discordEmbedField.Name+= DiscordEmoji.FromName(discordClient, $":{_enNums[i + 1].ToLower()}:")+ " ";
            discordEmbedField.Value += DiscordEmoji.FromName(discordClient, _dices[i] == 0 ? ":Empty:" : $":dice{_dices[i]}:") + " ";
        }
        embedBuilder.AddField(discordEmbedField);

        return embedBuilder;
    }
}