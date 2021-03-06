using System;
using System.Threading.Tasks;

namespace Imageboard10.Core.Tasks
{
    /// <summary>
    /// ����������� ���������� �������.
    /// </summary>
    /// <typeparam name="T">��� �������.</typeparam>
    /// <param name="sender">�������� �������.</param>
    /// <param name="e">�������.</param>
    /// <returns></returns>
    public delegate Task AsyncEventHandler<in T>(object sender, T e) where T : EventArgs;
}