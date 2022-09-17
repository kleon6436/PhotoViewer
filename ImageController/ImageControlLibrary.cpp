#include "pch.h"
/**
 * @file	ImageControlLibrary.cpp
 * @author	kleon6436
 */

#include "ImageControlLibrary.h"
#include "ImageReader.h"

namespace Kchary::ImageController::Library
{
	static ImageReader Reader;	//!< 画像読み込み用インスタンス

	bool LoadImageAndGetImageSize(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		return Reader.LoadImageAndGetImageSize(imagePath, imageReadSettings, imageSize);
	}

	bool GetImageData(ImageData& imageData)
	{
		return Reader.GetImageData(imageData);
	}

	bool GetThumbnailImageData(ImageData& imageData)
	{
		return Reader.GetThumbnailImageData(imageData);
	}
}