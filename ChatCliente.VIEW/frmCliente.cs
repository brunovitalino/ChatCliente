using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using ChatCliente.CONTROLLER;
using System.Threading;

namespace ChatCliente.VIEW
{
    //DELEGATE

    public delegate void AtualizaLogCallBack(string mensagem);


    //CLASS

    public partial class frmCliente : Form
    {
        //VARIÁVEIS GLOBAIS

        private bool _conectado = false;
        private CCliente _cliente = null;

        public bool Conectado
        {
            get { return _conectado; }
            set { _conectado = value; }
        }

        public CCliente Cliente
        {
            get { return _cliente; }
            set { _cliente = value; }
        }


        //CONSTRUTOR

        public frmCliente()
        {
            InitializeComponent();
        }


        //EVENTOS

        private void frmServidor_Load(object sender, EventArgs e)
        {
            //mskIp.ValidatingType = typeof(System.Net.IPAddress);
        }

        private void mskIp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Conectar();
            }
        }

        // Isso fará com que o "cursor" (ponto de inserção) se mova para a lacuna seguinte, que se inicia após o próximo ponto.
        // Cada vez que a tecla . (ponto) do teclado é apertada, esse salto será realizado.
        private void mskIp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals('.'))
            {
                for (int i = mskIpServidor.SelectionStart; i < mskIpServidor.MaskedTextProvider.Length; i++)
                {
                    if (!mskIpServidor.MaskedTextProvider.IsEditPosition(i))
                    {
                        mskIpServidor.SelectionStart = i;
                        // Após realizado o salto, não queremos que continue pelos campos seguintes.
                        // Então cancelamos as próximas iterações.
                        break;
                    }
                }
            }
        }

        private void mskIp_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            toolTip1.ToolTipTitle = "Entrada inválida";
            toolTip1.Show("Desculpe, somente digitos (0-9) são permitidos.", mskIpServidor, mskIpServidor.Location, 3000);
        }

        private void mskIp_MouseHover(object sender, EventArgs e)
        {
            // Caso o evento MaskInputRejected tenha sido disparado, o toolTip1 voltará ao padrão com o código abaixo.
            toolTip1.ToolTipTitle = "";
        }

        /*private void mskIp_Leave(object sender, EventArgs e)
        {
            // Reseta o cursor quando deixamos o maskedtextbox
            mskIp.SelectionStart = 0;
            // Habilita a propriedade TabStop assim nós podemos circular através dos form controls novamente  
            foreach (Control c in this.Controls)
            {
                c.TabStop = true;
            }
        }*/

        private void btnConectar_Click(object sender, EventArgs e)
        {
            Conectar();
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            Cliente.EnviarMensagem(txtMensagem.Text);
            txtMensagem.Text = "";
        }

        private void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.Invoke(new AtualizaLogCallBack(AtualizaLog), new object[] { e.MensagemEvento });
        }

        private void frmCliente_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Conectado)
            {
                Cliente.Conectar(false);
            }
            Application.Exit();
        }


        //MÉTODOS

        // Validação MaskedTextBox do mskIp
        // Se todas as lacunas do campo possui ao menos 1 posição preenchida cada, então o maskedtextbox estará validado.
        private bool isMascaraValidada()
        {
            // À cada 3 posições editáveis, significa que uma lacuna entre os pontos foi validada.
            int contador = 0;
            // Verifica se ao menos 1 posição, da lacuna que está sendo verificada, foi preenchida.
            bool espacoPreenchido = false;

            for (int i = 0; i < mskIpServidor.MaskedTextProvider.Length; i++)
            {
                // Com essa condição, toda a lógica será baseada sobre os campos editáveis apenas.
                if (mskIpServidor.MaskedTextProvider.IsEditPosition(i))
                {
                    // Atribuindo no inicio, fará com que o contador na sua primeira iteração já inicie com 1.
                    contador++;

                    // Verifica se esse espaço da lacuna não está vazio.
                    if (!mskIpServidor.GetCharFromPosition(mskIpServidor.GetPositionFromCharIndex(i)).Equals('_'))  // Temos que usar underline, pois a propriedade HidePrompOnLeave que torna os underlines em espaços em branco está quebrada.
                    {                                                                               // Pois fica preenchendo as últimas posições (que vêm após a última noEditPosition) com o dígito da primeira posição.
                        espacoPreenchido = true;
                    }

                    // Então verifique se algo foi digitado na lacuna.
                    if (contador >= 3)
                    {
                        // Se não houve nenhum espaço da lacuna preenchido, então:
                        if (!espacoPreenchido)
                        {
                            toolTip1.ToolTipTitle = "Endereço IP inválido";
                            toolTip1.Show("Desculpe, mas o endereço digitado não é válido. Digite um endereço válido.", mskIpServidor, 2000);
                            return false;
                        }
                        else
                        {
                            // Retornamos as variáveis aos valores padrões para realizarmos novas verificações.
                            contador = 0;
                            espacoPreenchido = false; //Como só é alterado por último, então não está disparando o ERRO acima que vem antes. Pois a iteração acaba PRIMEIRO
                        }
                    }
                }
            }

            return true;
        }

        // Conectar
        private void Conectar()
        {
            if (isMascaraValidada())
            {
                // Tenta se Conectar.
                if (!Conectado)
                {
                    // Conectando...
                    btnConectar.Enabled = false;
                    btnConectar.Text = "Conectando...";
                    btnConectar.BackColor = Color.Silver;
                    Cliente = new CCliente(mskIpServidor.Text.Replace(" ", ""), txtUsuario.Text.Replace(" ", ""));
                    CCliente.StatusChanged += new StatusChangedEventHandler(OnStatusChanged);
                    Cliente.Conectar(true);
                    Console.WriteLine("Conectado antes:" + Conectado);
                    Thread t = new Thread(RunDelay);
                    t.Start();
                    t.Join(); //Espera a finalização da thread t para que o restante do algoritmo possa dar prosseguimento.
                    // Atualiza o valor de Conectado da camada VIEW.
                    Conectado = Cliente.Conectado;
                    Console.WriteLine("Conectado dpois:" + Conectado);
                    if (!Conectado)
                    {
                        txtLog.AppendText("Não foi possível se conectar. ");
                    }
                }
                else
                {
                    // Desconectando...
                    btnConectar.Enabled = false;
                    btnConectar.Text = "Desconectando...";
                    btnConectar.BackColor = Color.Silver;
                    Cliente.Conectar(false);
                    Cliente = null;
                    Conectado = false;
                }

                // Resultado da tentativa de conexão.
                if (Conectado)
                {
                    Conectado = true;
                    btnConectar.Text = "Conectado";
                    btnConectar.ForeColor = Color.Blue;
                    btnConectar.BackColor = Color.Aqua;
                    txtLog.AppendText("Conectado.\r\n");
                }
                else
                {
                    Conectado = false;
                    Cliente = null;
                    btnConectar.Text = "Conectar";
                    btnConectar.ForeColor = Color.Aqua;
                    btnConectar.BackColor = Color.MediumBlue;
                    txtLog.AppendText("Desconectado.\r\n");
                }
                btnConectar.Enabled = true;
                Console.WriteLine("Conectado dpois2:" + Conectado);
                //MessageBox.Show("Erro de conexão : " + e.Message);
            }
        }

        private void AtualizaLog(string mensagem)
        {
            txtLog.AppendText(mensagem + "\r\n");
        }

        //
        private static void RunDelay()
        {
            Thread.Sleep(5000);
        }

    }
}
