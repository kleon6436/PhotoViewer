/**
 * @file	ImageControlLibrary
 * @brief	Image control library
 * @author	kchary6436
 * @date	2021/01/21
 */
#include "pch.h"

#include <memory>
#include <string>
#include "ImageControlLibrary.h"
#include "RawImageController.h"
#include "NormalImageController.h"

using namespace std;
using namespace Kchary::ImageController::RawImageControl;
using namespace Kchary::ImageController::NormalImageControl;

namespace Kchary::ImageController::Library
{
	int GetRawImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		unique_ptr<IImageController> rawImageController = make_unique<RawImageController>();
		return rawImageController->GetImageData(path, buffer, size, stride, width, height);
	}

	int GetRawThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		unique_ptr<IImageController> rawImageController = make_unique<RawImageController>();
		return rawImageController->GetThumbnailImageData(path, resizeLongSideLength, buffer, size, stride, width, height);
	}
	
	int GetNormalImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		unique_ptr<IImageController> normalImageController = make_unique<NormalImageController>();
		return normalImageController->GetImageData(path, buffer, size, stride, width, height);
	}

	int GetNormalThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
	{
		unique_ptr<IImageController> normalImageController = make_unique<NormalImageController>();
		return normalImageController->GetThumbnailImageData(path, resizeLongSideLength, buffer, size, stride, width, height);
	}

	void FreeBuffer(uint8_t* buffer)
	{
		delete[] buffer;
	}
}