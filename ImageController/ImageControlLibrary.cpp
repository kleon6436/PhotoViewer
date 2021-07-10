#include "pch.h"
/**
 * @file	ImageControlLibrary.cpp
 * @author	kleon6436
 */

#include <locale.h>
#include <memory>
#include "ImageControlLibrary.h"
#include "RawImageController.h"
#include "NormalImageController.h"

using namespace Kchary::ImageController::RawImageControl;
using namespace Kchary::ImageController::NormalImageControl;

namespace Kchary::ImageController::Library
{
	int GetRawImageData(const wchar_t imagePath[], ImageData& imageData)
	{
		const auto path = ConvertWcharToChar(imagePath);
		const std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetImageData(path.get(), imageData);
	}

	int GetRawThumbnailImageData(const wchar_t imagePath[], const int resizeLongSideLength, ImageData& imageData)
	{
		const auto path = ConvertWcharToChar(imagePath);
		const std::unique_ptr<IImageController> rawImageController = std::make_unique<RawImageController>();
		return rawImageController->GetThumbnailImageData(path.get(), resizeLongSideLength, imageData);
	}

	int GetNormalImageData(const wchar_t imagePath[], ImageData& imageData)
	{
		const auto path = ConvertWcharToChar(imagePath);
		const std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetImageData(path.get(), imageData);
	}

	int GetNormalThumbnailImageData(const wchar_t imagePath[], const int resizeLongSideLength, ImageData& imageData)
	{
		const auto path = ConvertWcharToChar(imagePath);
		const std::unique_ptr<IImageController> normalImageController = std::make_unique<NormalImageController>();
		return normalImageController->GetThumbnailImageData(path.get(), resizeLongSideLength, imageData);
	}

	void FreeBuffer(const std::uint8_t* buffer)
	{
		delete[] buffer;
	}

	std::unique_ptr<char[]> ConvertWcharToChar(const wchar_t imagePath[])
	{
		// This is the number of null characters.
		const auto imagePathLen = wcslen(imagePath) + 1;

		// Convert to byte number.
		const auto requestBufferSize = imagePathLen * 2;
		auto path = std::make_unique<char[]>(requestBufferSize);

		size_t convertedCharSize = 0;
		const auto locale = _create_locale(LC_CTYPE, "ja_JP.UTF-8");
		_wcstombs_s_l(&convertedCharSize, path.get(), requestBufferSize, imagePath, _TRUNCATE, locale);

		return std::move(path);
	}
}