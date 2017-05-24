using Windows.Foundation.Collections;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// Поддерживает конвертацию в тип <see cref="PropertySet"/>.
    /// </summary>
    public interface IPropertySetConvertable
    {
        /// <summary>
        /// Получить представление в виде <see cref="PropertySet"/>.
        /// </summary>
        /// <returns>Набор свойств.</returns>
        PropertySet AsPropertySet();
    }
}