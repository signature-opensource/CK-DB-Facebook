using CK.Core;

namespace CK.DB.User.UserFacebook
{
    /// <summary>
    /// Holds information stored for a Facebook user.
    /// </summary>
    public interface IUserFacebookInfo : IPoco
    {
        /// <summary>
        /// Gets or sets the Facebook account identifier.
        /// </summary>
        string FacebookAccountId { get; set; }
    }

}
