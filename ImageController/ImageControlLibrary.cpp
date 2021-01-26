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
	int GetRawImageData(const char* path, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetImageData(path, buffer, size, stride, width, height);
	}

	int GetRawThumbnailImageData(const char* path, int resizeLongSideLength, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetThumbnailImageData(path, resizeLongSideLength, buffer, size, stride, width, height);
	}
	
	int GetNormalImageData(const char* path, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetImageData(path, buffer, size, stride, width, height);
	}

	int GetNormalThumbnailImageData(const char* path, int resizeLongSideLength, std::uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetThumbnailImageData(path, resizeLongSideLength, buffer, size, stride, width, height);
	}

	void FreeBuffer(std::uint8_t* buffer)
	{
		delete[] buffer;
	}
}