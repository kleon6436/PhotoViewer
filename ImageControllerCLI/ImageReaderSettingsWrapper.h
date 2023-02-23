/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#pragma once

#include "ImageData.h"

public ref class ImageReaderSettingsWrapper
{
public:
	/*!
	* @brief コンストラクタ
	*/
	ImageReaderSettingsWrapper();

	/*!
	* @brief アンマネージド、マネージドリソースの開放
	*/
	~ImageReaderSettingsWrapper();

	/*!
	* @brief アンマネージドリソースの解放
	*/
	!ImageReaderSettingsWrapper();

	/// <summary>
	/// Raw画像フラグ
	/// </summary>
	property System::Boolean IsRawImage
	{
		System::Boolean get()
		{
			return m_imageReaderSettingsPtr->isRawImage;
		}
		void set(System::Boolean isRawImage)
		{
			m_imageReaderSettingsPtr->isRawImage = isRawImage;
		}
	}

	/// <summary>
	/// サムネイルモードフラグ
	/// </summary>
	property System::Boolean IsThumbnailMode
	{
		System::Boolean get()
		{
			return m_imageReaderSettingsPtr->isThumbnailMode;
		}
		void set(System::Boolean isThumbnailMode)
		{
			m_imageReaderSettingsPtr->isThumbnailMode = isThumbnailMode;
		}
	}

	/// <summary>
	/// リサイズ時の長辺側のサイズ
	/// </summary>
	property System::Int32 ResizeLongSideLength
	{
		System::Int32 get()
		{
			return m_imageReaderSettingsPtr->resizeLongSideLength;
		}
		void set(System::Int32 resizeLongSideLength)
		{
			m_imageReaderSettingsPtr->resizeLongSideLength = resizeLongSideLength;
		}
	}

internal:
	ImageReadSettings* m_imageReaderSettingsPtr;
};