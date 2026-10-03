using LibreHardwareMonitor.Hardware;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Minecraft.Source.UI
{
    public class DebugInfo : UIElement
    {
        private readonly FrameCounter _frameCounter;

        private readonly Text _textDrawFps;
        private readonly Text _textUpdateFps;
        private readonly Text _textBlocks;
        private readonly Text _textPos;
        private readonly Text _textDiagnostics;

        private readonly PerformanceCounter _cpuCounter;
        private readonly Computer _gpuComputer;
        private string _cachedDiagnostics = "";
        private readonly CancellationTokenSource _cts = new();

        public DebugInfo(FrameCounter frameCounter) : base(new Vector2(0, 0))
        {
            _frameCounter = frameCounter;
            AddChild(_textDrawFps = new Text(new Vector2(0, 0), ""));
            AddChild(_textUpdateFps = new Text(new Vector2(0, 13), ""));
            AddChild(_textBlocks = new Text(new Vector2(0, 26), ""));
            AddChild(_textPos = new Text(new Vector2(0, 39), ""));
            AddChild(_textDiagnostics = new Text(new Vector2(0, 52), ""));

            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue();

            _gpuComputer = new Computer { IsGpuEnabled = true };
            try
            {
                _gpuComputer.Open();
            }
            catch (Exception)
            {
                _gpuComputer = null;
            }

            Task.Run(() => UpdateDiagnosticsLoopAsync(_cts.Token));
        }

        private async Task UpdateDiagnosticsLoopAsync(CancellationToken token)
        {
            var visitor = new UpdateVisitor();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    float cpuUsage = _cpuCounter.NextValue();
                    double totalMemoryMB;

                    using (Process currentProcess = Process.GetCurrentProcess())
                    {
                        currentProcess.Refresh();

                        long totalMemory = currentProcess.WorkingSet64;

                        totalMemoryMB = totalMemory / (1024.0 * 1024.0);
                    }

                    float gpuUsage = 0;
                    float vramUsed = 0;

                    if (_gpuComputer != null)
                    {
                        _gpuComputer.Accept(visitor);
                        foreach (var hardware in _gpuComputer.Hardware)
                        {
                            if (hardware.HardwareType == HardwareType.GpuNvidia ||
                                hardware.HardwareType == HardwareType.GpuAmd ||
                                hardware.HardwareType == HardwareType.GpuIntel)
                            {
                                foreach (var sensor in hardware.Sensors)
                                {
                                    if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                                        gpuUsage = sensor.Value ?? 0;

                                    if (sensor.SensorType == SensorType.SmallData && sensor.Name == "GPU Memory Used")
                                        vramUsed = sensor.Value ?? 0;
                                }
                            }
                        }
                    }

                    string gpuString = _gpuComputer != null
                        ? $"GPU: {gpuUsage:F0}% VRAM: {vramUsed:F0}MB"
                        : "GPU: brak uprawnień administratora";

                    _cachedDiagnostics = $"CPU: {cpuUsage:F0}% Used RAM: {totalMemoryMB:F2}MB | {gpuString}";
                }
                catch (Exception ex)
                {
                    _cachedDiagnostics = $"Błąd diagnostyki: {ex.Message}";
                }

                await Task.Delay(1000, token);
            }
        }

        public void OnRenderData(int chunks, int visibleChunks, int vertices)
        {
            _textBlocks.SetText($"chunks: {chunks} (visible: {visibleChunks}) vertices: {vertices}");
        }

        public void OnPos(Vector3 pos)
        {
            _textPos.SetText($"{pos}");
        }

        public override void Update()
        {
            var drawFps = _frameCounter.GetDrawFps();
            var updateFps = _frameCounter.GetUpdateFps();
                
            _textDrawFps.SetText($"Draw FPS: {drawFps} (avg: {Math.Round(1000f / drawFps, 5)} ms)");
            _textUpdateFps.SetText($"Update FPS: {updateFps} (avg: {Math.Round(1000f / updateFps, 5)} ms)");
            _textDiagnostics.SetText(_cachedDiagnostics);

            base.Update();
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer) => computer.Traverse(this);
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (var sub in hardware.SubHardware) sub.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
    }
}
