namespace LaboratoireProgrammation.Project.Helpers;

public class ReactorCoreHelper {
    private readonly object _lockObject = new();
    private readonly Mutex _mutex = new();
    private readonly ReaderWriterLockSlim _rwLock = new();

    public int Pressure { get; private set; }

    public void ResetPressure() {
        Pressure = 0;
    }

    public void IncreasePressureUnsafe() {
        var temp = Pressure;
        Thread.Sleep(10);
        Pressure = temp + 1;
    }

    public void IncreasePressureLock() {
        lock (_lockObject) {
            var temp = Pressure;
            Thread.Sleep(10);
            Pressure = temp + 1;
        }
    }

    public void IncreasePressureRWLock() {
        _rwLock.EnterWriteLock();
        try {
            var temp = Pressure;
            Thread.Sleep(10);
            Pressure = temp + 1;
        }
        finally {
            _rwLock.ExitWriteLock();
        }
    }

    public void IncreasePressureMutex() {
        _mutex.WaitOne();
        try {
            var temp = Pressure;
            Thread.Sleep(10);
            Pressure = temp + 1;
        }
        finally {
            _mutex.ReleaseMutex();
        }
    }
}