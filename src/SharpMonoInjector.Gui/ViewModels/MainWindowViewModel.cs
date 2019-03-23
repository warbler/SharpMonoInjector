using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using SharpMonoInjector.Gui.Models;

namespace SharpMonoInjector.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public RelayCommand RefreshCommand { get; }

        public RelayCommand BrowseCommand { get; }

        public RelayCommand InjectCommand { get; }

        public RelayCommand EjectCommand { get; }

        public RelayCommand CopyStatusCommand { get; }

        public MainWindowViewModel()
        {
            RefreshCommand = new RelayCommand(ExecuteRefreshCommand, CanExecuteRefreshCommand);
            BrowseCommand = new RelayCommand(ExecuteBrowseCommand);
            InjectCommand = new RelayCommand(ExecuteInjectCommand, CanExecuteInjectCommand);
            EjectCommand = new RelayCommand(ExecuteEjectCommand, CanExecuteEjectCommand);
            CopyStatusCommand = new RelayCommand(ExecuteCopyStatusCommand);
        }

        private void ExecuteCopyStatusCommand(object parameter)
        {
            Clipboard.SetText(Status);
        }

        private bool CanExecuteRefreshCommand(object parameter)
        {
            return !IsRefreshing;
        }

        private async void ExecuteRefreshCommand(object parameter)
        {
            IsRefreshing = true;
            Status = "Refreshing processes";
            ObservableCollection<MonoProcess> processes = new ObservableCollection<MonoProcess>();

            await Task.Run(() =>
            {
                foreach (Process p in Process.GetProcesses()) {
                    const ProcessAccessRights flags = ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_VM_READ;
                    IntPtr handle;

                    if ((handle = Native.OpenProcess(flags, false, p.Id)) != IntPtr.Zero) {
                        if (ProcessUtils.GetMonoModule(handle, out IntPtr mono)) {
                            processes.Add(new MonoProcess
                            {
                                MonoModule = mono,
                                Id = p.Id,
                                Name = p.ProcessName
                            });
                        }

                        Native.CloseHandle(handle);
                    }
                }
            });

            Processes = processes;

            if (Processes.Count > 0)
                SelectedProcess = Processes[0];

            IsRefreshing = false;
            Status = "Processes refreshed";
        }

        private void ExecuteBrowseCommand(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dynamic Link Library|*.dll";
            ofd.Title = "Select assembly to inject";

            if (ofd.ShowDialog() == true)
                AssemblyPath = ofd.FileName;
        }

        private bool CanExecuteInjectCommand(object parameter)
        {
            return SelectedProcess != null &&
                File.Exists(AssemblyPath) &&
                !string.IsNullOrEmpty(InjectClassName) &&
                !string.IsNullOrEmpty(InjectMethodName) &&
                !IsExecuting;
        }

        private void ExecuteInjectCommand(object parameter)
        {
            IntPtr handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, SelectedProcess.Id);

            if (handle == IntPtr.Zero) {
                Status = "Failed to open process";
                return;
            }

            byte[] file;

            try {
                file = File.ReadAllBytes(AssemblyPath);
            } catch (IOException) {
                Status = "Failed to read the file " + AssemblyPath;
                return;
            }

            IsExecuting = true;
            Status = "Injecting " + Path.GetFileName(AssemblyPath);

            using (Injector injector = new Injector(handle, SelectedProcess.MonoModule)) {
                try {
                    IntPtr asm = injector.Inject(file, InjectNamespace, InjectClassName, InjectMethodName);
                    InjectedAssemblies.Add(new InjectedAssembly
                    {
                        ProcessId = SelectedProcess.Id,
                        Address = asm,
                        Name = Path.GetFileName(AssemblyPath),
                        Is64Bit = injector.Is64Bit
                    });
                    Status = "Injection successful";
                } catch (InjectorException ie) {
                    Status = "Injection failed: " + ie.Message;
                } catch (Exception e) {
                    Status = "Injection failed (unknown error): " + e.Message;
                }
            }

            IsExecuting = false;
        }

        private bool CanExecuteEjectCommand(object parameter)
        {
            return SelectedAssembly != null &&
                !string.IsNullOrEmpty(EjectClassName) &&
                !string.IsNullOrEmpty(EjectMethodName) &&
                !IsExecuting;
        }

        private void ExecuteEjectCommand(object parameter)
        {
            IntPtr handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, SelectedAssembly.ProcessId);

            if (handle == IntPtr.Zero) {
                Status = "Failed to open process";
                return;
            }

            IsExecuting = true;
            Status = "Ejecting " + SelectedAssembly.Name;

            ProcessUtils.GetMonoModule(handle, out IntPtr mono);

            using (Injector injector = new Injector(handle, mono)) {
                try {
                    injector.Eject(SelectedAssembly.Address, EjectNamespace, EjectClassName, EjectMethodName);
                    InjectedAssemblies.Remove(SelectedAssembly);
                    Status = "Ejection successful";
                } catch (InjectorException ie) {
                    Status = "Ejection failed: " + ie.Message;
                } catch (Exception e) {
                    Status = "Ejection failed (unknown error): " + e.Message;
                }
            }

            IsExecuting = false;
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set {
                Set(ref _isRefreshing, value);
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isExecuting;
        public bool IsExecuting
        {
            get => _isExecuting;
            set {
                Set(ref _isExecuting, value);
                InjectCommand.RaiseCanExecuteChanged();
                EjectCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<MonoProcess> _processes;
        public ObservableCollection<MonoProcess> Processes
        {
            get => _processes;
            set => Set(ref _processes, value);
        }

        private MonoProcess _selectedProcess;
        public MonoProcess SelectedProcess
        {
            get => _selectedProcess;
            set {
                _selectedProcess = value;
                InjectCommand.RaiseCanExecuteChanged();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private string _assemblyPath;
        public string AssemblyPath
        {
            get => _assemblyPath;
            set {
                Set(ref _assemblyPath, value);
                if (File.Exists(_assemblyPath))
                    InjectNamespace = Path.GetFileNameWithoutExtension(_assemblyPath);
                InjectCommand.RaiseCanExecuteChanged();
            }
        }

        private string _injectNamespace;
        public string InjectNamespace
        {
            get => _injectNamespace;
            set {
                Set(ref _injectNamespace, value);
                EjectNamespace = value;
            }
        }

        private string _injectClassName;
        public string InjectClassName
        {
            get => _injectClassName;
            set {
                Set(ref _injectClassName, value);
                EjectClassName = value;
                InjectCommand.RaiseCanExecuteChanged();
            }
        }

        private string _injectMethodName;
        public string InjectMethodName
        {
            get => _injectMethodName;
            set {
                Set(ref _injectMethodName, value);
                if (_injectMethodName == "Load")
                    EjectMethodName = "Unload";
                InjectCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<InjectedAssembly> _injectedAssemblies = new ObservableCollection<InjectedAssembly>();
        public ObservableCollection<InjectedAssembly> InjectedAssemblies
        {
            get => _injectedAssemblies;
            set => Set(ref _injectedAssemblies, value);
        }

        private InjectedAssembly _selectedAssembly;
        public InjectedAssembly SelectedAssembly
        {
            get => _selectedAssembly;
            set {
                Set(ref _selectedAssembly, value);
                EjectCommand.RaiseCanExecuteChanged();
            }
        }

        private string _ejectNamespace;
        public string EjectNamespace
        {
            get => _ejectNamespace;
            set => Set(ref _ejectNamespace, value);
        }

        private string _ejectClassName;
        public string EjectClassName
        {
            get => _ejectClassName;
            set {
                Set(ref _ejectClassName, value);
                EjectCommand.RaiseCanExecuteChanged();
            }
        }

        private string _ejectMethodName;
        public string EjectMethodName
        {
            get => _ejectMethodName;
            set {
                Set(ref _ejectMethodName, value);
                EjectCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
