using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading.SmartStandards {

  public static class MultiTask {

    public delegate bool TryGetNextItemMethod<TItem>(out TItem nextItem);

    public static void RunAndWait<TItem>(TryGetNextItemMethod<TItem> nextItemGetter, int numberOfThreads, Action<TItem> processorMethod) {
      Run<TItem>(nextItemGetter, numberOfThreads, processorMethod).Wait();
    }

    public static void RunAndWait<TItem>(TryGetNextItemMethod<TItem> nextItemGetter, int numberOfThreads, Action<TItem> processorMethod, CancellationToken cancellationToken) {
      Run<TItem>(nextItemGetter, numberOfThreads, processorMethod).Wait(CancellationToken.None);
    }

    public static Task Run<TItem>(TryGetNextItemMethod<TItem> nextItemGetter, int numberOfThreads, Action<TItem> processorMethod) {
      var tasks = new List<Task>();
      TryGetNextItemMethod<TItem> syncLockedItemGetter = (
        (out TItem nextItem) => {
          lock (tasks) {
            return nextItemGetter.Invoke(out nextItem);
          }
        }
       );
      for (int blockIndex = 0; blockIndex < numberOfThreads; blockIndex++) {
        int offset = blockIndex;
        tasks.Add(
          Task.Run(
            () => {
              Thread.CurrentThread.Name = $"Block#{offset}";
              while (syncLockedItemGetter.Invoke(out TItem nextItem)) {
                processorMethod.Invoke(nextItem);
              }
            }
          )
        );
      }
      return Task.WhenAll(tasks.ToArray());
    }

    public static void RunAndWait<TItem>(IEnumerable<TItem> itemsToProcess, int numberOfThreads, Action<TItem> processorMethod) {
      Run<TItem>(itemsToProcess, numberOfThreads, processorMethod).Wait();
    }

    public static void RunAndWait<TItem>(IEnumerable<TItem> itemsToProcess, int numberOfThreads, Action<TItem> processorMethod, CancellationToken cancellationToken) {
      Run<TItem>(itemsToProcess, numberOfThreads, processorMethod).Wait(CancellationToken.None);
    }

    public static Task Run<TItem>(IEnumerable<TItem> itemsToProcess, int numberOfThreads, Action<TItem> processorMethod) {
      IEnumerator<TItem> enumerator = itemsToProcess.GetEnumerator();
      TryGetNextItemMethod<TItem> nextItemGetter = (
        (out TItem nextItem) => {
          //lock (enumerator) { //not neccessarry, because there is already a semaphore which locks the nextItemGetter call
          if (!enumerator.MoveNext()) {
            nextItem = default(TItem);
            return false;
          }
          nextItem = enumerator.Current;
          return true;
          //}
        }
      );
      return Run<TItem>(nextItemGetter, numberOfThreads, processorMethod);
    }

    public static void RunAndWait<TItem>(TItem[] itemsToProcess, int numberOfThreads, Action<TItem> processorMethod) {
      Run<TItem>(itemsToProcess, numberOfThreads, processorMethod).Wait();
    }

    public static void RunAndWait<TItem>(TItem[] itemsToProcess, int numberOfThreads, Action<TItem> processorMethod, CancellationToken cancellationToken) {
      Run<TItem>(itemsToProcess, numberOfThreads, processorMethod).Wait(CancellationToken.None);
    }

    public static Task Run<TItem>(TItem[] itemsToProcess, int numberOfThreads, Action<TItem> processorMethod) {
      int inputLength = itemsToProcess.Length;
      var tasks = new List<Task>();
      for (int blockIndex = 0; blockIndex < numberOfThreads; blockIndex++) {
        int offset = blockIndex;
        tasks.Add(
          Task.Run(
            () => {
              Thread.CurrentThread.Name = $"Block#{offset}";
              for (int i = offset; i < inputLength; i += numberOfThreads) {
                processorMethod.Invoke(itemsToProcess[i]);
              }
            }
          )
        );
      }
      return Task.WhenAll(tasks);
    }

  }

}