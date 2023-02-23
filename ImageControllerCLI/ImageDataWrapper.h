/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#pragma once

#include "ImageData.h"

public ref class ImageDataWrapper
{
public:
	/*!
	* @brief コンストラクタ
	*/
	ImageDataWrapper();

	/*!
	* @brief アンマネージド、マネージドリソースの開放
	*/
	~ImageDataWrapper();

	/*!
	* @brief アンマネージドリソースの解放
	*/
	!ImageDataWrapper();

	/// <summary>
	/// 画像のバッファ
	/// </summary>
	property array<System::Byte>^ Buffer
	{
		array<System::Byte>^ get()
		{
			if (!m_imageDataPtr || m_imageDataPtr->buffer.empty())
			{
				return nullptr;
			}

			auto buffer = gcnew array<System::Byte>(m_imageDataPtr->buffer.size());
			System::Runtime::InteropServices::Marshal::Copy(System::IntPtr(m_imageDataPtr->buffer.data()), buffer, 0, buffer->Length);

			return buffer;
		}
	}

	/// <summary>
	/// 画像バッファサイズ
	/// </summary>
	property System::UInt32 BufferSize
	{
		System::UInt32 get()
		{
			return m_imageDataPtr->size;
		}
	}

	/// <summary>
	/// ストライド
	/// </summary>
	property System::Int32 Stride
	{
		System::Int32 get()
		{
			return m_imageDataPtr->stride;
		}
	}

	/// <summary>
	/// 画像の幅
	/// </summary>
	property System::Int32 Width
	{
		System::Int32 get()
		{
			return m_imageDataPtr->width;
		}
	}

	/// <summary>
	/// 画像の高さ
	/// </summary>
	property System::Int32 Height
	{
		System::Int32 get()
		{
			return m_imageDataPtr->height;
		}
	}

internal:
	ImageData* m_imageDataPtr;
};