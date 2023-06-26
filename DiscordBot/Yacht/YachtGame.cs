using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DiscordBot.Commands;

namespace DiscordBot.Yacht;

public enum EYachtPointType : ushort 
{
    Aces,
    Deuces,
    Threes,
    Fours,
    Fives,
    Sixes,
    SubTotal,
    Bonus,
    Choice,
    FourOfaKind,
    FullHouse,
    SStraight,
    LStraight,
    Yacht,
    Total,
}

public class YachtGame
{
    private const int A_CODE = 0x1F1E6;
    public DiscordUser? _1P;
    public DiscordUser? _2P;
    public DiscordThreadChannel? _yachtChannel;
    private DiscordMessage? _yachtScoreUiMessage;
    private DiscordMessage? _yachtDiceTrayUiMessage;

    private int _turn = 1;
    private int _diceChance = 3;

    private readonly int?[,] _points = new int?[2, 15];
    private readonly int[] _tempPoints = new int[15];
    private readonly int[] _dices = new int[5];

    private readonly bool[] _diceTarget = new bool[5];

    public DiscordUser? CurrPlayer => _turn % 2 == 0 ? _2P : _1P;
    private string TurnPlayer => _turn % 2 == 0 ? "2P" : "1P";
    private int TurnPlayerNum  => _turn % 2 == 0 ? 1 : 0;
    private int Round => _turn / 2 + _turn % 2 / 1;
    
    

    private async Task<bool> SetUi(DiscordClient discordClient)
    {
        if (_yachtDiceTrayUiMessage == null && _yachtScoreUiMessage == null)
        {
            _yachtScoreUiMessage = await _yachtChannel?.SendMessageAsync(ScoreBoardVisualize(discordClient))!;

            for (int i = 0; i < 'm' - 'a'; i++)
            {
                await _yachtScoreUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode(Utility.GetRegionalIndicatorSymbolLetter(i)));
            }

            _yachtDiceTrayUiMessage = await _yachtChannel?.SendMessageAsync(DiceTrayVisualize(discordClient))!; 
            await _yachtDiceTrayUiMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("🆕"));
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

        for (int i = 0; i < (int)EYachtPointType.SubTotal; i++)
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
        int turn = TurnPlayerNum;
        for (int i = 0; i < 6; i++)
        {
            topSum += _points[turn, i] ?? 0;
        }

        _points[turn,(int)EYachtPointType.SubTotal] = topSum;
        _points[turn, (int)EYachtPointType.Bonus] = topSum >= 63 ? 35 : 0;
        
        int totalSum = 0;
        for (int i = (int)EYachtPointType.SubTotal; i < (int)EYachtPointType.Total; i++)
        {
            totalSum += _points[turn, i] ?? 0;
        }
        _points[turn,(int)EYachtPointType.Total] = totalSum;
       
    }
    public async Task GameSettle()
    {
        int player1Total = _points[0, (int)EYachtPointType.Total] ?? 0;
        int player2Total = _points[1, (int)EYachtPointType.Total] ?? 0;
        YachtModules.RemoveChannel(_yachtChannel!.Id);
        if (player1Total == player2Total)
        {
            await _yachtChannel?.SendMessageAsync("Draw")!;
            return;
        }

        await _yachtChannel?.SendMessageAsync($"Player {(player1Total < player2Total ? _2P : _1P)?.Username} Win")!;
    }
    public async Task ChoicePoint(DiscordClient discordClient, EYachtPointType eYachtPointType)
    {
        if (_points[TurnPlayerNum, (int)eYachtPointType] == null)
            _points[TurnPlayerNum, (int)eYachtPointType] = _tempPoints[(int)eYachtPointType];
        else
            return;
        
        DiceReset();
        PointTempSettle();
        PointSettle();
        _turn++;
        await _yachtDiceTrayUiMessage?.DeleteAllReactionsAsync()!;
        await _yachtDiceTrayUiMessage?.CreateReactionAsync(DiscordEmoji.FromUnicode("🆕"))!;
        
        
        await RefreshGameBoard(discordClient);
    }
    public async Task RefreshGameBoard(DiscordClient discordClient)
    {
        if (!await SetUi(discordClient))
        {
            return;
        }
        Optional<DiscordEmbed> scoreEmbed = Optional.Some<DiscordEmbed>(ScoreBoardVisualize(discordClient));
        await _yachtScoreUiMessage?.ModifyAsync(scoreEmbed)!;
        Optional<DiscordEmbed> diceTrayEmbed = Optional.Some<DiscordEmbed>(DiceTrayVisualize(discordClient));
        await _yachtDiceTrayUiMessage?.ModifyAsync(diceTrayEmbed)!;
        
        if (Round > 12)
        {
            await _yachtDiceTrayUiMessage?.DeleteAsync()!;
            _yachtDiceTrayUiMessage = null;
            await GameSettle();
        }
    }
    public async Task DiceTrayMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtDiceTrayUiMessage!.Id)
            return;
            
        if (CurrPlayer?.Id != eventArgs.User.Id)
            return;

        if (eventArgs.Emoji.Name == "🆕")
        {
            DiceRoll(true);
            await RefreshGameBoard(client);
            await eventArgs.Message.DeleteReactionsEmojiAsync(eventArgs.Emoji);
            for (int i = 0; i < 'f' - 'a'; i++)
            {
                await eventArgs.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(Utility.GetRegionalIndicatorSymbolLetter(i)));
            }

            await eventArgs.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🔄"));
            return;
        }

        if (eventArgs.Emoji.Name == "🔄")
        {
            await eventArgs.Message.DeleteReactionsEmojiAsync(eventArgs.Emoji);
            DiceRoll(false);
            await RefreshGameBoard(client);
            if (_diceChance > 0)
                await eventArgs.Message.CreateReactionAsync(eventArgs.Emoji);
            return;
        }

        int emojiToIndex = char.ConvertToUtf32(eventArgs.Emoji, 0) - 0x1F1E6;
        _diceTarget[emojiToIndex] = true;
    }
    public async Task DiceTrayMessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;

        if (eventArgs.Message.Id != _yachtDiceTrayUiMessage!.Id)
            return;

        if (CurrPlayer?.Id != eventArgs.User.Id)
            return;
        
        
        int emojiToIndex = char.ConvertToUtf32(eventArgs.Emoji, 0) - A_CODE;
        _diceTarget[emojiToIndex] = false;
    }
    public async Task ScoreBoardMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (_yachtChannel!.Id != eventArgs.ChannelId)
            return;
        
        if (eventArgs.Message.Id != _yachtScoreUiMessage!.Id)
            return;

        if (CurrPlayer?.Id != eventArgs.User.Id)
            return;


        int index = 0;
        int emojiToIndex = char.ConvertToUtf32(eventArgs.Emoji, 0) - 0x1F1E6;

        foreach (EYachtPointType yachtPointType in Enum.GetValues(typeof(EYachtPointType)))
        {
            bool bIsSumField = false;
            bIsSumField |= yachtPointType == EYachtPointType.SubTotal;
            bIsSumField |= yachtPointType == EYachtPointType.Bonus;
            bIsSumField |= yachtPointType == EYachtPointType.Total;
            if (bIsSumField)
                continue;

            if (emojiToIndex == index)
            {
                await ChoicePoint(client, yachtPointType);
                break;
            }

            index++;
        }
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
        int diceTargetCount = 0;
        for (int i = 0 ; i<_dices.Length;i++)
        {
            if (!_diceTarget[i] && !forceRoll)
                continue;

            diceTargetCount++;
            _dices[i] = rand.Next(1, 7);
        }

        if (diceTargetCount > 0)
        {
            _diceChance--;
            PointTempSettle();
        }
    }
    void DiceReset()
    {
        _diceChance = 3;
        for (int i = 0; i < _dices.Length; i++)
        {
            _dices[i] = 0;
            _diceTarget[i] = false;
        }
    }
    private DiscordEmbedBuilder ScoreBoardVisualize(DiscordClient discordClient)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .WithTitle("ScoreBoard");
        string field01 = string.Empty; 
        string field02 = string.Empty;
        string field03 = string.Empty;


        int alphabetIndex = 0;
        string normalBorder = "\n──────────\n"; 
        foreach (EYachtPointType yachtPointType in Enum.GetValues(typeof(EYachtPointType)))
        {
            int pointType = (int)yachtPointType;
            bool bIsSumField = false;
            bIsSumField |= yachtPointType == EYachtPointType.SubTotal;
            bIsSumField |= yachtPointType == EYachtPointType.Bonus;
            bIsSumField |= yachtPointType == EYachtPointType.Total;
            
            field01 += (!bIsSumField ? Utility.GetRegionalIndicatorSymbolLetter(alphabetIndex) : string.Empty) + yachtPointType + normalBorder;
            field02 += (_points[0, pointType] != null ? $"[{_points[0, pointType].ToString()}]" : TurnPlayerNum == 0 ? DiscordEmoji.FromUnicode("➡️") + _tempPoints[pointType] : "[empty]") + normalBorder;
            field03 += (_points[1, pointType] != null ? $"[{_points[1, pointType].ToString()}]" : TurnPlayerNum == 1 ? DiscordEmoji.FromUnicode("➡️") + _tempPoints[pointType] : "[empty]") + normalBorder;
            if (!bIsSumField)
                alphabetIndex++;
        }

        embedBuilder.AddField(new DiscordEmbedField($"ROUNDS {Round}", field01, true));
        embedBuilder.AddField(new DiscordEmbedField($"1P : {_1P?.Username}", field02, true));
        embedBuilder.AddField(new DiscordEmbedField($"2P : {_2P?.Username}", field03, true));

        return embedBuilder;
    }
    private DiscordEmbedBuilder DiceTrayVisualize(DiscordClient discordClient)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Green)
            .WithTitle($"{TurnPlayer} : {CurrPlayer?.Username}  ReRoll Count {_diceChance}")
            .AddField(new DiscordEmbedField("DiceIndex:", "Dice:"/*+"\nDiceValue:"*/, true));

        string name = string.Empty;
        string diceEmojis = string.Empty;
        string diceValues = "\n";
        
        for (int i = 0; i < _dices.Length; i++)
        {
            name += DiscordEmoji.FromUnicode(Utility.GetRegionalIndicatorSymbolLetter(i)) + " ";
            diceEmojis += DiscordEmoji.TryFromName(discordClient, _dices[i] == 0 ? ":Empty:" : $":dice{_dices[i]}:", out var discordDiceEmoji) ? discordDiceEmoji + " " : string.Empty;
            diceValues += DiscordEmoji.FromUnicode(Utility.GetNumEmoji(_dices[i])) + " ";
        }

        if (string.IsNullOrEmpty(diceEmojis))
            diceEmojis = diceValues;
        else
            diceEmojis += diceValues;
        
        embedBuilder.AddField(new DiscordEmbedField(name, diceEmojis, true));

        return embedBuilder;
    }
}