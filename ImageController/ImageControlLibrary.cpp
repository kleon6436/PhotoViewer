#include "pch.h"
/**
 * @file	ImageControlLibrary.cpp
 * @author	kleon6436
 */

#include "ImageControlLibrary.h"
#include "ImageReader.h"

namespace Kchary::ImageController::Library
{
	ImageReader* CreateInstance()
	{
		return new ImageReader();
	}

	void DeleteInstance(ImageReader* reader)
	{
		delete reader;
	}

	bool LoadImageAndGetImageSize(ImageReader* reader, const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		return reader->LoadImageAndGetImageSize(imagePath, imageReadSettings, imageSize);
	}

	bool GetImageData(ImageReader* reader, ImageData& imageData)
	{
		return reader->GetImageData(imageData);
	}

	bool GetThumbnailImageData(ImageReader* reader, ImageData& imageData)
	{
		return reader->GetThumbnailImageData(imageData);
	}
}