using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCliente.CONTROLLER
{
    public class StatusChangedEventArgs : EventArgs
    {
        //VARIÁVEIS GLOBAIS

        private string _mensagemEvento;

        public string MensagemEvento
        {
            get { return _mensagemEvento; }
            set { _mensagemEvento = value; }
        }

        //CONSTRUTOR

        public StatusChangedEventArgs(string mensagemEvento)
        {
            MensagemEvento = mensagemEvento;
        }
    }
}
