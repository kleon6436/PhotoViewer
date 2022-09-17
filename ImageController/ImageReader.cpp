#include "pch.h"

/*!
 * @file	ImageReader.cpp
 * @author	kleon6436
 *
 */

#include "RawImageController.h"
#include "NormalImageController.h"
#include "ImageReader.h"
#include <iostream>

namespace Kchary::ImageController::Library
{
	using namespace Kchary::ImageController::RawImageControl;
	using namespace Kchary::ImageController::NormalImageControl;

	ImageReader::ImageReader()
	{
		m_rawImageController = std::make_unique<RawImageController>();
		m_normalImageController = std::make_unique<NormalImageController>();
	}

	bool ImageReader::LoadImageAndGetImageSize(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		const auto path = ConvertWcharToChar(imagePath);
		m_imageReadSettings = imageReadSettings;

		bool result;
		if (imageReadSettings.isRawImage)
		{
			result = m_rawImageController->LoadImageAndGetImageSize(path.get(), imageReadSettings, imageSize);
		}
		else
		{
			result = m_normalImageController->LoadImageAndGetImageSize(path.get(), imageReadSettings, imageSize);
		}

		return result;
	}

	bool ImageReader::GetImageData(ImageData& imageData)
	{
		bool result;
		if (m_imageReadSettings.isRawImage)
		{
			result = m_rawImageController->GetImageData(imageData);
		}
		else
		{
			result = m_normalImageController->GetImageData(imageData);
		}

		return result;
	}

	bool ImageReader::GetThumbnailImageData(ImageData& imageData)
	{
		if (!m_imageReadSettings.isThumbnailMode)
		{
			// サムネイルモードで画像を読み込んでいないのでエラーとする
			return false;
		}

		if (m_imageReadSettings.isRawImage)
		{
			m_rawImageController->GetThumbnailImageData(imageData);
		}
		else
		{
			m_normalImageController->GetThumbnailImageData(imageData);
		}

		return true;
	}

	std::unique_ptr<char[]> ImageReader::ConvertWcharToChar(const wchar_t* imagePath)
	{
		setlocale(LC_CTYPE, "ja_JP.UTF-8");

		// This is the number of null characters.
		const auto imagePathLen = wcslen(imagePath) + 1;

		// Convert to byte number.
		const auto requestBufferSize = imagePathLen * 2;
		auto path = std::make_unique<char[]>(requestBufferSize);

		size_t convertedCharSize = 0;
		wcstombs_s(&convertedCharSize, path.get(), requestBufferSize, imagePath, _TRUNCATE);

		return path;
	}
}