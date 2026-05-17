using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace PLC_Control
{
    public partial class MainWindow : Window
    {
        private PLCController? _plc;

        public MainWindow()
        {
            InitializeComponent();
            UpdateConnectionUi();
        }

        private void UpdateConnectionUi()
        {
            bool connected = _plc != null;
            ConnectButton.IsEnabled = !connected;
            DisconnectButton.IsEnabled = connected;
            StationTextBox.IsEnabled = !connected;
            SetZeroButton.IsEnabled = connected;
            MoveButton.IsEnabled = connected;
            BackMoveButton.IsEnabled = connected;
            ResetZeroFlagButton.IsEnabled = connected;
        }

        private void SetStatus(string message)
        {
            StatusTextBlock.Text = message;
        }

        private bool TryParseStation(out int station)
        {
            station = 0;
            if (!int.TryParse(StationTextBox.Text.Trim(), out int s) || s < 0)
            {
                MessageBox.Show(this, "请输入有效的非负整数作为逻辑站号。", "逻辑站号无效",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            station = s;
            return true;
        }

        private void MainWindow_OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _plc?.Close();
            _plc = null;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseStation(out int station))
                return;

            ConnectButton.IsEnabled = false;
            try
            {
                PLCController plc;
                try
                {
                    plc = new PLCController(station);
                }
                catch (COMException ex) when ((uint)ex.HResult == 0x80040154)
                {
                    MessageBox.Show(this,
                        "无法创建 MX Component 控件（0x80040154 类未注册）。\n\n"
                        + "请确认：1）已安装三菱 MX Component；2）本项目须以 x86（32 位）平台运行——经典 ActUtlType 与默认的 64 位 AnyCPU 不匹配。\n\n"
                        + "若必须使用 64 位进程，请改用 ActUtlType64 并调整 COM 引用与类型名。",
                        "COM 未注册",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    SetStatus("未连接（COM 类未注册）");
                    return;
                }

                bool ok = await Task.Run(() => PLCFunction.OpenPLC(plc)).ConfigureAwait(true);
                if (ok)
                {
                    _plc?.Close();
                    _plc = plc;
                    SetStatus($"已连接（逻辑站号 {station}）");
                }
                else
                {
                    plc.Close();
                    MessageBox.Show(this, "连接失败，请检查 MX Component 逻辑站配置与运行时。", "连接失败",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    SetStatus("未连接（连接失败）");
                }
            }
            finally
            {
                UpdateConnectionUi();
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _plc?.Close();
            _plc = null;
            SetStatus("未连接");
            UpdateConnectionUi();
        }

        private async void SetZeroButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null)
                return;

            SetActionButtonsEnabled(false);
            try
            {
                bool ok = await Task.Run(() => PLCFunction.SetZeroPoint(_plc)).ConfigureAwait(true);
                SetStatus(ok
                    ? "设定原点：成功"
                    : "设定原点：失败（请确认已连接且设备可写）");
            }
            finally
            {
                UpdateConnectionUi();
            }
        }

        private async void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null)
                return;

            if (!int.TryParse(FrequencyTextBox.Text.Trim(), out int freq) || freq <= 0)
            {
                MessageBox.Show(this, "请输入有效的正整数作为频率值（D0）。", "频率值无效",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TargetPositionTextBox.Text.Trim(), out int step))
            {
                MessageBox.Show(this, "请输入有效的整数作为目标位置（D1）。", "目标位置无效",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SetActionButtonsEnabled(false);
            try
            {
                bool ok = await Task.Run(() => PLCFunction.Move(_plc, freq, step)).ConfigureAwait(true);
                SetStatus(ok
                    ? $"移动：成功（频率 {freq}，目标 {step}）"
                    : "移动：失败（需已设定原点且 M10=ON，或通信/写入异常）");
            }
            finally
            {
                UpdateConnectionUi();
            }
        }

        private async void BackMoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null)
                return;

            if (!int.TryParse(FrequencyTextBox.Text.Trim(), out int freq) || freq <= 0)
            {
                MessageBox.Show(this, "请输入有效的正整数作为频率值（D0）。", "频率值无效",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TargetPositionTextBox.Text.Trim(), out int step))
            {
                MessageBox.Show(this, "请输入有效的整数作为目标位置（D1）。", "目标位置无效",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SetActionButtonsEnabled(false);
            try
            {
                bool ok = await Task.Run(() => PLCFunction.BackMove(_plc, freq, step)).ConfigureAwait(true);
                SetStatus(ok
                    ? $"回原点：成功（频率 {freq}，目标 {step}）"
                    : "回原点：失败（需已设定原点且 M10=ON，或通信/写入异常）");
            }
            finally
            {
                UpdateConnectionUi();
            }
        }

        private async void ResetZeroFlagButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null)
                return;

            SetActionButtonsEnabled(false);
            try
            {
                bool ok = await Task.Run(() => PLCFunction.ResetZeroPointFlag(_plc)).ConfigureAwait(true);
                SetStatus(ok
                    ? "急停成功（已写 M3）"
                    : "急停失败（请确认已连接）");
            }
            finally
            {
                UpdateConnectionUi();
            }
        }

        private void SetActionButtonsEnabled(bool enabled)
        {
            SetZeroButton.IsEnabled = enabled && _plc != null;
            MoveButton.IsEnabled = enabled && _plc != null;
            BackMoveButton.IsEnabled = enabled && _plc != null;
            ResetZeroFlagButton.IsEnabled = enabled && _plc != null;
        }
    }
}