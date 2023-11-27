using XA_DATASETLib;
using XA_SESSIONLib;

namespace Test
{
    public partial class Form1 : Form
    {
        private XASession _xaSession;
        private XAQuery _xaQuery;
        public Form1()
        {
            InitializeComponent();

            _xaSession = new XASession();
            _xaQuery = new XAQuery();

            ((_IXASessionEvents_Event)_xaSession).Disconnect += XaSessionOnDisconnect;
            ((_IXASessionEvents_Event)_xaSession).Login += OnLogin;
            ((_IXASessionEvents_Event)_xaSession).Logout += OnLogout;

            _xaSession.ConnectServer("ebestsec.co.kr", 20001);
            _xaSession.Login("tksgo199", "1q2w3e4r", "rhksgnl11!#@", 0, true);


        }

        private void OnLogout()
        {
            throw new NotImplementedException();
        }

        private void OnLogin(string szcode, string szmsg)
        {
            throw new NotImplementedException();
        }

        private void XaSessionOnDisconnect()
        {
            throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
    }
}