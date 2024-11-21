using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;
using CK.DB.Auth;
using CK.DB.Auth.AuthScope;
using FluentAssertions;
using CK.Testing;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserFacebook.AuthScope.Tests;

[TestFixture]
public class UserFacebookAuthScopeTests
{

    [Test]
    public async Task non_user_facebook_ScopeSet_is_null_Async()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            var id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
            (await p.ReadScopeSetAsync( ctx, id )).Should().BeNull();
        }
    }

    [Test]
    public async Task setting_default_scopes_impact_new_usersl_Async()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        var factory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserFacebookInfo>>();
        using( var ctx = new SqlStandardCallContext() )
        {
            AuthScopeSet original = await p.ReadDefaultScopeSetAsync( ctx );
            original.Contains( "nimp" ).Should().BeFalse();
            original.Contains( "thing" ).Should().BeFalse();
            original.Contains( "other" ).Should().BeFalse();

            {
                int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                IUserFacebookInfo userInfo = factory.Create();
                userInfo.FacebookAccountId = Guid.NewGuid().ToString();
                await p.UserFacebookTable.CreateOrUpdateFacebookUserAsync( ctx, 1, id, userInfo );
                var info = await p.UserFacebookTable.FindKnownUserInfoAsync( ctx, userInfo.FacebookAccountId );
                AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, info.UserId );
                userSet.ToString().Should().Be( original.ToString() );
            }
            AuthScopeSet replaced = original.Clone();
            replaced.Add( new AuthScopeItem( "nimp" ) );
            replaced.Add( new AuthScopeItem( "thing", ScopeWARStatus.Rejected ) );
            replaced.Add( new AuthScopeItem( "other", ScopeWARStatus.Accepted ) );
            await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, replaced );
            var readback = await p.ReadDefaultScopeSetAsync( ctx );
            readback.ToString().Should().Be( replaced.ToString() );
            // Default scopes have non W status!
            // This must not impact new users: their satus must always be be W.
            readback.ToString().Should().Contain( "[R]thing" )
                                        .And.Contain( "[A]other" );

            {
                int userId = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                IUserFacebookInfo userInfo = p.UserFacebookTable.CreateUserInfo<IUserFacebookInfo>();
                userInfo.FacebookAccountId = Guid.NewGuid().ToString();
                await p.UserFacebookTable.CreateOrUpdateFacebookUserAsync( ctx, 1, userId, userInfo, UCLMode.CreateOnly | UCLMode.UpdateOnly );

                AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, userId );
                userSet.ToString().Should().Contain( "[W]thing" )
                                           .And.Contain( "[W]other" )
                                           .And.Contain( "[W]nimp" );
            }
            await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, original );
        }
    }

}

