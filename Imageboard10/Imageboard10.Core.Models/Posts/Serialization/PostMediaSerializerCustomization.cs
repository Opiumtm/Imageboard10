using Imageboard10.Core.Models.Posts.PostMedia;
using Imageboard10.Core.Models.Serialization;

namespace Imageboard10.Core.Models.Posts.Serialization
{
    /// <summary>
    /// ������������ ������������ ������� <see cref="PostMedia"/>.
    /// </summary>
    public class PostMediaSerializerCustomization<T> : ObjectSerializerCustomization<T>
        where T : PostMedia.PostMedia, new()
    {
        /// <summary>
        /// ��������� �������� ����� �������������.
        /// </summary>
        /// <param name="obj">�������� ������.</param>
        /// <returns>����������� ������.</returns>
        public override T ValidateContract(T obj)
        {
            obj = base.ValidateContract(obj);
            if (obj != null)
            {
                obj.MediaLinkContract = obj.MediaLink != null ? LinkSerializationService.Serialize(obj.MediaLink) : null;
            }
            return obj;
        }

        /// <summary>
        /// ��������� �������� ����� ������������.
        /// </summary>
        /// <param name="obj">�������� ������.</param>
        /// <returns>����������� ������.</returns>
        public override T ValidateAfterDeserialize(T obj)
        {
            obj = base.ValidateAfterDeserialize(obj);
            if (obj != null)
            {
                obj.MediaLink = obj.MediaLinkContract != null ? LinkSerializationService.Deserialize(obj.MediaLinkContract) : null;
                obj.MediaLinkContract = null;
            }
            return obj;
        }
    }
}