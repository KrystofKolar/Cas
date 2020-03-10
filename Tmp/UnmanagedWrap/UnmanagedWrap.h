#pragma once
#include "Unmanaged.h"

namespace SandBox
{
	public class UnmanagedWrapNoRef
	{
		Unmanaged *pUnmanged;

		UnmanagedWrapNoRef()
			: pUnmanged(new Unmanaged())
		{
		};

		int HelloForwarded()
		{
			return pUnmanged->Hello();
		}
	};

	// this is now a managed class
	public ref class UnmanagedWrap
	{
	public:
		Unmanaged *pUnmanged;

		UnmanagedWrap()
			: pUnmanged(new Unmanaged())
		{

		};

		int HelloForwarded()
		{
			return pUnmanged->Hello();
		}

		void DeleteMe()
		{
			this->~UnmanagedWrap();
		}

		~UnmanagedWrap()
		{
		}
	};
}