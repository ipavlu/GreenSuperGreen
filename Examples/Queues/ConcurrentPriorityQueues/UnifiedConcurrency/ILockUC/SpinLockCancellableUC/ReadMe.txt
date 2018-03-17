Examples:
SpinLockCancellableUC spinlock = new SpinLockCancellableUC(CancellationToken cancellationToken);

using (EntryBlockUC entry = spinlock.TryEnter())//instantly gets acces or no entry
{
	Assert.AreEqual(entry.EntryTypeUC, EntryTypeUC.Exclusive);
	Assert.IsTrue(entry.HasEntry);
	if (entry.HasEntry)
	{
		//spin-lock protected section
	}
}

using (EntryBlockUC entry = spinlock.TryEnter(1))//trying to get acces for 1ms or no entry
{
	Assert.AreEqual(entry.EntryTypeUC, EntryTypeUC.Exclusive);
	Assert.IsTrue(entry.HasEntry);
	if (entry.HasEntry)
	{
		//spin-lock protected section
	}
}

using (spinlock.Enter())    //Enter
{
	//spin-lock protected section
}


using (EntryBlockUC entry = spinlock.Enter())//Enter
{
	//spin-lock protected section
}

//TryEnter - Enters only when right now nobody has access,
using (EntryBlockUC entry = spinlock.TryEnter())
{
	if (entry.HasEntry)
	{
		//spin-lock protected section
	}
}

//TryEnter - Enters only if entering does not take longer than 5ms in this example
using (EntryBlockUC entry = spinlock.TryEnter(5))
{
	if (entry.HasEntry)
	{
		//spin-lock protected section
	}
}
