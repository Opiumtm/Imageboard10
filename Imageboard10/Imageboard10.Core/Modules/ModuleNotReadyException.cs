using System;

namespace Imageboard10.Core.Modules
{
    /// <summary>
    /// ������: ������ �� ����� � �������������.
    /// </summary>
    public class ModuleNotReadyException : Exception
    {
        /// <summary>
        /// �����������.
        /// </summary>
        public ModuleNotReadyException()
            :base("������ �� ����� � ������������� - �� ���������������, �������� ��� �������������")
        {            
        }
    }
}