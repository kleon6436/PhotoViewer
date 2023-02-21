#include "pch.h"
/**
 * @file	NormalImageController.cpp
 * @author	kleon6436
 */

#include "NormalImageController.h"

namespace Kchary::ImageController::NormalImageControl
{
	bool NormalImageController::LoadImageAndGetImageSize(const wchar_t* path, const ImageReadSettings& imageReadSettings, int& imageSize)
	{
		// OpenCVは、wchar_t -> charに変換しないと読めないので、ここで変換処理を行う
		const auto pathStr = std::string(ConvertWcharToString(path));

		if (imageReadSettings.isThumbnailMode)
		{
			// Read image data to mat.
			auto image = cv::imread(pathStr, GetImreadMode(imageReadSettings.resizeLongSideLength));

			// Resize image data.
			const auto longSideLength = image.cols > image.rows ? image.cols : image.rows;
			const auto ratio = (static_cast<double>(imageReadSettings.resizeLongSideLength) / static_cast<double>(longSideLength));
			cv::Mat resizeImg;
			cv::resize(image, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

			m_image = resizeImg;
			imageSize = static_cast<int>(resizeImg.total() * resizeImg.elemSize() * sizeof(std::uint8_t));
		}
		else
		{
			// Read image data to mat.
			m_image = cv::imread(pathStr, cv::ImreadModes::IMREAD_COLOR);
			imageSize = static_cast<int>(m_image.total() * m_image.elemSize() * sizeof(std::uint8_t));
		}

		return true;
	}

	bool NormalImageController::GetImageData(ImageData& imageData)
	{
		if (m_image.empty())
		{
			return false;
		}

		const auto dataSize = m_image.total() * m_image.elemSize();
		memcpy(imageData.buffer, m_image.data, dataSize * sizeof(std::uint8_t));

		// Translate data to C#
		imageData.size = static_cast<unsigned int>(dataSize);
		imageData.stride = static_cast<int>(m_image.step);
		imageData.width = m_image.cols;
		imageData.height = m_image.rows;

		return true;
	}

	bool NormalImageController::GetThumbnailImageData(ImageData& imageData)
	{
		if (m_image.empty())
		{
			return false;
		}

		const auto dataSize = m_image.total() * m_image.elemSize();
		memcpy(imageData.buffer, m_image.data, dataSize * sizeof(std::uint8_t));

		// Translate data to C#
		imageData.size = static_cast<unsigned int>(dataSize);
		imageData.stride = static_cast<int>(m_image.step);
		imageData.width = m_image.cols;
		imageData.height = m_image.rows;

		return true;
	}

	cv::ImreadModes NormalImageController::GetImreadMode(const int resizeLongSideLength)
	{
		cv::ImreadModes imreadMode;

		if (resizeLongSideLength <= 800)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
		}
		else if (800 < resizeLongSideLength && resizeLongSideLength <= 1600)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
		}
		else if (1600 < resizeLongSideLength && resizeLongSideLength <= 3200)
		{
			imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
		}
		else
		{
			imreadMode = cv::ImreadModes::IMREAD_COLOR;
		}

		return imreadMode;
	}

	std::string NormalImageController::ConvertWcharToString(const wchar_t* imagePath)
	{
		setlocale(LC_CTYPE, "ja_JP.UTF-8");

		std::wstring wide(imagePath);
		int bufferSize = WideCharToMultiByte(CP_OEMCP, 0, wide.c_str(), -1, (char*)NULL, 0, NULL, NULL);

		auto* cpMultiByte = new CHAR[bufferSize];
		WideCharToMultiByte(CP_OEMCP, 0, wide.c_str(), -1, cpMultiByte, bufferSize, NULL, NULL);
		std::string imagePathStr(cpMultiByte, cpMultiByte + bufferSize - 1);
		delete[] cpMultiByte;

		// 変換結果を返す
		return imagePathStr;
	}
}