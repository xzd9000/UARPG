using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IActivatable
{
    void Activate();
    void Activate(object obj);
    void Activate(object obj1, object obj2);
    void Activate(object obj1, object obj2, object obj3);
    void Activate(object obj1, object obj2, object obj3, object obj4);
    void Activate(params object[] args);
    void Activate(object sender, EventArgs args);
}

