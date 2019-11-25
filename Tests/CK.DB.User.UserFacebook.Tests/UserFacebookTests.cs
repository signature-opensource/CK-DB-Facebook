using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;
using CK.DB.Auth;
using System.Collections.Generic;
using FluentAssertions;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.User.UserFacebook.Tests
{
    [TestFixture]
    public class UserFacebookTests
    {
        [Test]
        public void create_Facebook_user_and_check_read_info_object_method()
        {
            var u = TestHelper.StObjMap.StObjs.Obtain<UserFacebookTable>();
            var user = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            var infoFactory = TestHelper.StObjMap.StObjs.Obtain<IPocoFactory<IUserFacebookInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.FacebookAccountId = googleAccountId;
                var created = u.CreateOrUpdateFacebookUser( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = u.FindKnownUserInfo( ctx, googleAccountId );

                info2.UserId.Should().Be( userId );
                info2.Info.FacebookAccountId.Should().Be( googleAccountId );

                u.FindKnownUserInfo( ctx, Guid.NewGuid().ToString() ).Should().BeNull();
                user.DestroyUser( ctx, 1, userId );
                u.FindKnownUserInfo( ctx, googleAccountId ).Should().BeNull();
            }
        }

        [Test]
        public async Task create_Facebook_user_and_check_read_info_object_method_async()
        {
            var u = TestHelper.StObjMap.StObjs.Obtain<UserFacebookTable>();
            var user = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            var infoFactory = TestHelper.StObjMap.StObjs.Obtain<IPocoFactory<IUserFacebookInfo>>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var googleAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.FacebookAccountId = googleAccountId;
                var created = await u.CreateOrUpdateFacebookUserAsync( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = await u.FindKnownUserInfoAsync( ctx, googleAccountId );

                info2.UserId.Should().Be( userId );
                info2.Info.FacebookAccountId.Should().Be( googleAccountId );

                (await u.FindKnownUserInfoAsync( ctx, Guid.NewGuid().ToString() )).Should().BeNull();
                await user.DestroyUserAsync( ctx, 1, userId );
                (await u.FindKnownUserInfoAsync( ctx, googleAccountId )).Should().BeNull();
            }
        }

        [Test]
        public void Facebook_AuthProvider_is_registered()
        {
            Auth.Tests.AuthTests.CheckProviderRegistration( "Facebook" );
        }

        [Test]
        public void vUserAuthProvider_reflects_the_user_Facebook_authentication()
        {
            var u = TestHelper.StObjMap.StObjs.Obtain<UserFacebookTable>();
            var user = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Facebook auth - " + Guid.NewGuid().ToString();
                var googleAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Facebook'" )
                    .Rows.Should().BeEmpty();
                var info = u.CreateUserInfo<IUserFacebookInfo>();
                info.FacebookAccountId = googleAccountId;
                u.CreateOrUpdateFacebookUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='Facebook'" )
                    .Should().Be( 1 );
                u.DestroyFacebookUser( ctx, 1, idU );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Facebook'" )
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void standard_generic_tests_for_Facebook_provider()
        {
            var auth = TestHelper.StObjMap.StObjs.Obtain<Auth.Package>();
            // With IUserFacebookInfo POCO.
            var f = TestHelper.StObjMap.StObjs.Obtain<IPocoFactory<IUserFacebookInfo>>();
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "Facebook",
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.FacebookAccountId = "FacebookAccountIdFor:" + userName ),
                payloadForLogin: ( userId, userName ) => f.Create( i => i.FacebookAccountId = "FacebookAccountIdFor:" + userName ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => i.FacebookAccountId = "NO!" + userName )
                );
            // With a KeyValuePair.
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "Facebook",
                payloadForCreateOrUpdate: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "FacebookAccountId", "IdFor:" + userName)
                },
                payloadForLogin: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "FacebookAccountId", "IdFor:" + userName)
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "FacebookAccountId", ("IdFor:" + userName).ToUpperInvariant())
                }
                );
        }

        [Test]
        public async Task standard_generic_tests_for_Facebook_provider_Async()
        {
            var auth = TestHelper.StObjMap.StObjs.Obtain<Auth.Package>();
            var f = TestHelper.StObjMap.StObjs.Obtain<IPocoFactory<IUserFacebookInfo>>();
            await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
                auth,
                "Facebook",
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.FacebookAccountId = "FacebookAccountIdFor:" + userName ),
                payloadForLogin: ( userId, userName ) => f.Create( i => i.FacebookAccountId = "FacebookAccountIdFor:" + userName ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => i.FacebookAccountId = "NO!" + userName )
                );
        }

    }

}

