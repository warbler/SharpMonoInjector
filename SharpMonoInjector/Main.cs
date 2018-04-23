using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SharpMonoInjector.Injection;

namespace SharpMonoInjector
{
    public partial class Main : Form
    {
        private Injector _injector;

        public Main() {InitializeComponent();}

        private void Main_Load(object sender, EventArgs e)
        {
            RefreshProcesses();
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

            if (_injector == null || _injector.ProcessHandle != target.Process.Handle)
                _injector = new Injector(target.Process.Handle);

            if (!Injector.ExportsLoaded)
            {
                if (!Injector.LoadMonoFunctions(target.Process.Modules.Cast<ProcessModule>()
                    .First(pm => pm.ModuleName == "mono.dll").FileName))
                {
                    OnError("Failed to load mono.dll");
                    return;
                }
            }

            string assembly = txtAssembly.Text;

            if (!Utils.ReadFile(assembly, out byte[] bytes))
            {
                OnError("Failed to read the specified assembly");
                return;
            }

            var config = new InjectionConfig
            {
                Assembly = bytes,
                AssemblyPath = assembly,
                Namespace = txtNamespace.Text,
                Class = txtClass.Text,
                Method = txtMethod.Text
            };

            try
            {
                _injector.Inject(config);
                lstInjected.Items.Add(config);
            }
            catch (ApplicationException ae)
            {
                OnError($"Injection failed: {ae.Message}");
            }
            catch (Exception ex)
            {
                OnError($"An unknown error occurred: {ex.Message}");
            }
        }

        private void btnEject_Click(object sender, EventArgs e)
        {
            InjectionConfig config = (InjectionConfig)lstInjected.SelectedItem;

            if (config != null)
            {
                try
                {
                    _injector.UnloadAndCloseAssembly(config, txtUnload.Text);
                    lstInjected.Items.Remove(config);
                }
                catch (ApplicationException ae)
                {
                    OnError($"Ejection failed: {ae.Message}");
                }
                catch (Exception ex)
                {
                    OnError($"An unknown error occurred: {ex.Message}");
                }
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show($"ERROR: {message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
