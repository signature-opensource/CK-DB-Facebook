using CK.Core;

namespace CK.DB.User.UserFacebook
{
    /// <summary>
    /// Package that adds Facebook authentication support for users. 
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions("1.0.0")]
    [SqlObjectItem( "transform:vUserAuthProvider" )]
    public class Package : SqlPackage
    {
        void StObjConstruct( Actor.Package actorPackage, Auth.Package authPackage )
        {
        }

        /// <summary>
        /// Gets the user Facebook table.
        /// </summary>
        [InjectObject]
        public UserFacebookTable UserFacebookTable { get; protected set; }

    }
}
