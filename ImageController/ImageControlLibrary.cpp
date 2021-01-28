#include "pch.h"
/**
 * @file	ImageControlLibrary.cpp
 * @author	kchary6436
 */

#include <memory>
#include <string>
#include "ImageControlLibrary.h"
#include "RawImageController.h"
#include "NormalImageController.h"

using namespace Kchary::ImageController::RawImageControl;
using namespace Kchary::ImageController::NormalImageControl;

namespace Kchary::ImageController::Library
{
	int GetRawImageData(const char* path, ImageData* imageData)
	{
		const std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetImageData(path, imageData);
	}

	int GetRawThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData)
	{
		const std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetThumbnailImageData(path, resizeLongSideLength, imageData);
	}
	
	int GetNormalImageData(const char* path, ImageData* imageData)
	{
		const std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetImageData(path, imageData);
	}

	int GetNormalThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData)
	{
		const std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetThumbnailImageData(path, resizeLongSideLength, imageData);
	}

	void FreeBuffer(std::uint8_t* buffer)
	{
		delete[] buffer;
	}
}