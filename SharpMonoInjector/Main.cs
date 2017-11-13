using System;
using System.IO;
using System.Windows.Forms;

namespace SharpMonoInjector
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            cbProcesses.DisplayMember = "Text";
            lstInjected.DisplayMember = "Text";
            RefreshProcesses();
            Injector.Instance.Error += OnError;
            Injector.Instance.SuccessfulInjection += OnSuccessfulInjection;
            Injector.Instance.SuccessfulEjection += OnSuccessfulEjection;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Injector.Instance.Dispose();
        }

        private void lblRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RefreshProcesses();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Dynamic Link Library|*.dll";
                dialog.Title = "Select assembly to inject";

                if (dialog.ShowDialog() == DialogResult.OK)
                    txtAssembly.Text = dialog.FileName;
            }
        }

        private void lstInjected_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool valid = lstInjected.SelectedIndex >= 0;
            btnEject.Enabled = valid;
            txtUnload.Enabled = valid;
        }

        private void txtAssembly_TextChanged(object sender, EventArgs e)
        {
            btnInject.Enabled = File.Exists(txtAssembly.Text);
        }

        private void txtUnload_Enter(object sender, EventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (t.Text == "Unload method name")
                t.Clear();
        }

        private void txtUnload_Leave(object sender, EventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (string.IsNullOrEmpty(t.Text))
                t.Text = "Unload method name";
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            MonoProcess target = (MonoProcess)cbProcesses.SelectedItem;
            if (target == null)
                return;

            target.Process.Refresh();
            if (target.Process.HasExited)
            {
                OnError("The target process has exited");
                return;
            }

            string assembly, name_space, klass, method;

            if (string.IsNullOrEmpty((assembly = txtAssembly.Text)))
                return;
            if (string.IsNullOrEmpty((name_space = txtNamespace.Text)))
                return;
            if (string.IsNullOrEmpty((klass = txtClass.Text)))
                return;
            if (string.IsNullOrEmpty((method = txtMethod.Text)))
                return;

            byte[] data = null;
            try { data = File.ReadAllBytes(assembly); }
            catch (Exception ex)
            {
                OnError($"Could not read the file '{assembly}': {ex.Message}");
                return;
            }

            try
            {
                Injector.Instance.Inject(new InjectionConfig
                {
                    Process = target.Process,
                    Assembly = data,
                    AssemblyPath = assembly,
                    Namespace = name_space,
                    Class = klass,
                    Method = method
                });
            }
            catch (Exception ex)
            {
                OnError($"An error occurred during injection: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private void btnEject_Click(object sender, EventArgs e)
        {
            InjectionConfig config = (InjectionConfig)lstInjected.SelectedItem;

            if (config != null)
            {
                string unloadMethod = txtUnload.Text;
                Injector.Instance.UnloadAndCloseAssembly(
                    config, unloadMethod != "Unload method name" ? unloadMethod : null);
                lstInjected.Items.Remove(config);
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show($"ERROR: {message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnSuccessfulInjection(InjectionConfig config)
        {
            lstInjected.Items.Add(config);
        }

        private void OnSuccessfulEjection(InjectionConfig config)
        {
            lstInjected.Items.Remove(config);
        }

        private void RefreshProcesses()
        {
            cbProcesses.Items.Clear();
            cbProcesses.ResetText();
            cbProcesses.Items.AddRange(MonoProcess.GetProcesses());
            if (cbProcesses.Items.Count > 0)
                cbProcesses.SelectedIndex = 0;
        }
    }
}
