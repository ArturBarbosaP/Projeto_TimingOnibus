using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimingOnibus
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptInterface
    {
        private FormPrincipal form;

        public ScriptInterface(FormPrincipal form)
        {
            this.form = form;
        }

        public void SelecionarPonto(string idPonto)
        {
            form.OnPontoSelecionado(idPonto);
        }
    }
}