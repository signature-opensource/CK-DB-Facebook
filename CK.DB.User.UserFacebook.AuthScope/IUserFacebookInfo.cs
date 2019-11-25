using CK.Core;
using System;

namespace CK.DB.User.UserFacebook.AuthScope
{
    /// <summary>
    /// Extends <see cref="UserFacebook.IUserFacebookInfo"/> with ScopeSet identifier.
    /// </summary>
    public interface IUserFacebookInfo : UserFacebook.IUserFacebookInfo
    {
        /// <summary>
        /// Gets the scope set identifier.
        /// Note that the ScopeSetId is intrinsic: a new ScopeSetId is acquired 
        /// and set only when a new UserFacebook is created (by copy from 
        /// the default one - the ScopeSet of the UserFacebook 0).
        /// </summary>
        int ScopeSetId { get; }
    }
}
