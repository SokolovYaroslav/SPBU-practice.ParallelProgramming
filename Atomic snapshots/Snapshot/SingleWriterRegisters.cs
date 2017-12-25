using System.Linq;
using System.Text;
using System.Threading;

namespace Snapshot
{
    public class SingleWriterRegisters
    {
        public readonly int RegistersAmount;
        private int _operationCounter = 0;
        private static Logger _logger;

        private readonly bool[,] _q;
        private readonly Register[] _regs;
        
        private struct Register
        {
            public int Value;
            public int[] Snapshot;
            public bool Toggle;
            public bool[] P;
        }

        public SingleWriterRegisters(int registersAmount, Logger logger) {
            RegistersAmount = registersAmount;
            _logger = logger;
            _regs = new Register[RegistersAmount];
            _q = new bool[RegistersAmount, RegistersAmount];
            for (var i = 0; i < RegistersAmount; i++) {
                _regs[i] = new Register
                {
                    P = new bool[RegistersAmount],
                    Snapshot = new int[RegistersAmount]
                };
            }
        }

        public int[] Scan(int id) {
            var moved = new bool[RegistersAmount];

            while (true) {
                for (var j = 0; j < RegistersAmount; j++) {
                    _q[id, j] = _regs[j].P[id];
                }

                var a = CopyRegs();
                var b = CopyRegs();

                var flag = true;
                for (var j = 0; j < RegistersAmount; j++) {
                    if (a[j].P[id] == b[j].P[id] && a[j].P[id] == _q[id, j]
                                    && a[j].Toggle == b[j].Toggle) continue;
                    flag = false;
                    break;
                }
                
                int operationNumber;
                
                if (flag) {
                    operationNumber = Interlocked.Increment(ref _operationCounter);
                    
                    var values = b.Select(register => register.Value).ToArray();
                    
                    var stringBuilder = new StringBuilder();
                    foreach (var value in values) {
                        stringBuilder.Append($"{value} ");
                    }
                    
                    _logger.WriteToLog(operationNumber, $"scan {stringBuilder}");
                    
                    return values;
                }

                for (var j = 0; j < RegistersAmount; j++) {
                    if (a[j].P[id] == _q[id, j] && b[j].P[id] == _q[id, j] 
                                    && a[j].Toggle == b[j].Toggle) continue;
                    if (moved[j]) {
                        operationNumber = Interlocked.Increment(ref _operationCounter);
                        
                        var values = b[j].Snapshot;
                    
                        var stringBuilder = new StringBuilder();
                        foreach (var value in values) {
                            stringBuilder.Append($"{value} ");
                        }
                    
                        _logger.WriteToLog(operationNumber, $"scan {stringBuilder}");
                        
                        return values;
                    }
                    moved[j] = true;
                }
            }
        }

        public void Update(int id, int value) {
            int operationNumber;
            
            var f = new bool[RegistersAmount];
            for (var j = 0; j < RegistersAmount; j++) {
                f[j] = !_q[j, id];
            }

            var snapshot = Scan(id);

            _regs[id].Value = value;
            _regs[id].P = f;
            _regs[id].Toggle = !_regs[id].Toggle;
            _regs[id].Snapshot = snapshot;
            
            operationNumber = Interlocked.Increment(ref _operationCounter);
            _logger.WriteToLog(operationNumber, $"write in cell #{id} {value}");
        }

        private Register[] CopyRegs() {
            var regsCopy = new Register[RegistersAmount];
            for (var i = 0; i < RegistersAmount; i++) {
                regsCopy[i] = _regs[i];
            }

            return regsCopy;
        }
    }
}