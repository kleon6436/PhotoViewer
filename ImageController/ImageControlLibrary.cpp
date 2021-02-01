#include "pch.h"
/**
 * @file	ImageControlLibrary.cpp
 * @author	kchary6436
 */

#include <stdlib.h>
#include <memory>
#include "ImageControlLibrary.h"
#include "RawImageController.h"
#include "NormalImageController.h"

using namespace Kchary::ImageController::RawImageControl;
using namespace Kchary::ImageController::NormalImageControl;

namespace Kchary::ImageController::Library
{
	int GetRawImageData(const wchar_t* wpath, ImageData* imageData)
	{
		const auto wpathLen = wcslen(wpath) + 1; // This is the number of null characters.
		const auto requestBufferSize = wpathLen * 2;	// Convert to byte number.
		const auto path = std::make_unique<char[]>(requestBufferSize);

		if (ConvertWcharToChar(wpath, requestBufferSize, path.get()) != 0)
		{
			return -1;
		}

		const std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetImageData(path.get(), imageData);
	}

	int GetRawThumbnailImageData(const wchar_t* wpath, const int resizeLongSideLength, ImageData* imageData)
	{
		const auto wpathLen = wcslen(wpath) + 1; // This is the number of null characters.
		const auto requestBufferSize = wpathLen * 2;	// Convert to byte number.
		const auto path = std::make_unique<char[]>(requestBufferSize);

		if (ConvertWcharToChar(wpath, requestBufferSize, path.get()) != 0)
		{
			return -1;
		}

		const std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetThumbnailImageData(path.get(), resizeLongSideLength, imageData);
	}

	int GetNormalImageData(const wchar_t* wpath, ImageData* imageData)
	{
		const auto wpathLen = wcslen(wpath) + 1; // This is the number of null characters.
		const auto requestBufferSize = wpathLen * 2;	// Convert to byte number.
		const auto path = std::make_unique<char[]>(requestBufferSize);

		if (ConvertWcharToChar(wpath, requestBufferSize, path.get()) != 0)
		{
			return -1;
		}

		const std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetImageData(path.get(), imageData);
	}

	int GetNormalThumbnailImageData(const wchar_t* wpath, const int resizeLongSideLength, ImageData* imageData)
	{
		const auto wpathLen = wcslen(wpath) + 1; // This is the number of null characters.
		const auto requestBufferSize = wpathLen * 2;	// Convert to byte number.
		const auto path = std::make_unique<char[]>(requestBufferSize);

		if (ConvertWcharToChar(wpath, requestBufferSize, path.get()) != 0)
		{
			return -1;
		}

		const std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetThumbnailImageData(path.get(), resizeLongSideLength, imageData);
	}

	void FreeBuffer(const std::uint8_t* buffer)
	{
		delete[] buffer;
	}

	int ConvertWcharToChar(const wchar_t* wpath, const size_t requestBufferSize, char* path)
	{
		setlocale(LC_CTYPE, "ja_JP.UTF-8"); // Handle character strings in Japanese.

		size_t convertedCharSize = 0;
		return wcstombs_s(&convertedCharSize, path, requestBufferSize, wpath, _TRUNCATE);
	}
}