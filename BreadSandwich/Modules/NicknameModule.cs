using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace BreadSandwich.Core.Modules
{
    public class NicknameModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<NicknameModule> _logger;

        public NicknameModule(ILogger<NicknameModule> logger)
        {
            _logger = logger;
        }
        
        //[RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [Command("nickname")]
        [Summary("Swaps certain strings for nicknames.")]
        public async Task SwapStringsInAllNicknames([Summary("First string to swap")] string string1, [Summary("First string to swap")] string string2)
        {
            var dictionary = new Dictionary<string, string>();
            var statusMessage = "";

            const string illegalDiscordString = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var userLists = await Context.Guild.GetUsersAsync().ToListAsync();
            foreach (var userList in userLists)
            {
                foreach (var user in userList)
                {
                    var nickname = user.Nickname;
                    if (nickname == null)
                    {
                        nickname = user.Username;
                    }

                    var newNickname = nickname.Replace(string1, illegalDiscordString);
                    newNickname = newNickname.Replace(string2, string1);
                    newNickname = newNickname.Replace(illegalDiscordString, string2);
                    dictionary[nickname] = newNickname;
                    try
                    {
                        await user.ModifyAsync(x => { x.Nickname = newNickname; });
                    }
                    catch (Exception e)
                    {
                        statusMessage +=
                            $"\nInsufficient permissions to change name of {nickname} to {newNickname}.";
                    }
                }
            }


            foreach (var oldName in dictionary.Keys)
            {
                statusMessage += $"\nTried to change {oldName} to {dictionary[oldName]}";
            }

            _logger.LogInformation(statusMessage);

            await Context.Channel.SendMessageAsync(statusMessage);
        }
    }
}